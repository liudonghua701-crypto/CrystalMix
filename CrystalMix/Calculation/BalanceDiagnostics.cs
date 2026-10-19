using System.Globalization;

namespace CrystalMix.Calculation;

internal static class BalanceDiagnostics
{
    // 消元时保留元素方程的线性组合，给出可核对的守恒冲突。
    public static string? Find(double[,] a, double[] b, string[] species)
    {
        int rows = b.Length, cols = a.GetLength(1), pivotRow = 0;
        var m = (double[,])a.Clone();
        var witness = new double[rows, rows];
        for (int r = 0; r < rows; r++) witness[r, r] = 1;
        for (int c = 0; c < cols && pivotRow < rows; c++)
        {
            int pivot = pivotRow;
            for (int r = pivotRow + 1; r < rows; r++)
                if (Math.Abs(m[r, c]) > Math.Abs(m[pivot, c])) pivot = r;
            if (Math.Abs(m[pivot, c]) < 1e-10) continue;
            for (int j = 0; j < cols; j++) (m[pivotRow, j], m[pivot, j]) = (m[pivot, j], m[pivotRow, j]);
            for (int j = 0; j < rows; j++) (witness[pivotRow, j], witness[pivot, j]) = (witness[pivot, j], witness[pivotRow, j]);
            double divisor = m[pivotRow, c];
            for (int j = 0; j < cols; j++) m[pivotRow, j] /= divisor;
            for (int j = 0; j < rows; j++) witness[pivotRow, j] /= divisor;
            for (int r = 0; r < rows; r++)
            {
                if (r == pivotRow) continue;
                double factor = m[r, c];
                for (int j = 0; j < cols; j++) m[r, j] -= factor * m[pivotRow, j];
                for (int j = 0; j < rows; j++) witness[r, j] -= factor * witness[pivotRow, j];
            }
            pivotRow++;
        }
        for (int r = pivotRow; r < rows; r++)
        {
            double residual = Enumerable.Range(0, rows).Sum(j => witness[r, j] * b[j]);
            double scale = Math.Max(1, Enumerable.Range(0, rows).Sum(j => Math.Abs(witness[r, j] * b[j])));
            if (Math.Abs(residual) <= 1e-8 * scale) continue;
            int element = Enumerable.Range(0, rows).Where(j => Math.Abs(witness[r, j]) > 1e-10)
                .OrderBy(j => Enumerable.Range(0, rows).Count(k => k != j && witness[r, k] / witness[r, j] > 1e-10)).First();
            double implied = b[element] - residual / witness[r, element];
            var terms = Enumerable.Range(0, rows).Where(j => j != element && Math.Abs(witness[r, j]) > 1e-10)
                .Select(j => $"{Number(-witness[r, j] / witness[r, element])}×{species[j]}");
            string relation = string.Join(" + ", terms);
            if (relation.Length == 0) relation = "0";
            return $"所选原料存在元素守恒冲突 / Element balance conflict:\n{species[element]} = {relation}\n"
                + $"目标 / Target {species[element]}: {Number(b[element])}; 按其他目标元素所需 / Required: {Number(implied)}.\n"
                + "请核对目标化学式或更换原料；此检查不判断合成路线。 / Check the target formula or selected materials; this is a composition check.";
        }
        return null;
    }
    private static string Number(double value) => value.ToString("G8", CultureInfo.InvariantCulture);
}
