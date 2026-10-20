param(
    [string]$JavaDirectory = (Join-Path (Split-Path $PSScriptRoot) '.tools\android-jdk'),
    [string]$Destination = (Join-Path (Split-Path $PSScriptRoot) '.signing')
)
$ErrorActionPreference='Stop'
$key=Join-Path $Destination 'crystalmix-android.keystore'
$storePassword=Join-Path $Destination 'android-password.txt'
$keyPassword=Join-Path $Destination 'android-key-password.txt'
foreach($file in @($key,$storePassword,$keyPassword)){
    if(Test-Path -LiteralPath $file){throw 'Signing files already exist. Refusing to replace an update identity.'}
}
$keytool=Join-Path $JavaDirectory 'bin\keytool.exe'
if(-not(Test-Path -LiteralPath $keytool)){throw 'JDK keytool not found.'}
New-Item -ItemType Directory -Force $Destination | Out-Null
$bytes=New-Object byte[] 32
$rng=[Security.Cryptography.RandomNumberGenerator]::Create()
$rng.GetBytes($bytes);$rng.Dispose()
$password=[Convert]::ToBase64String($bytes)
[IO.File]::WriteAllText($storePassword,$password)
[IO.File]::WriteAllText($keyPassword,$password)
& $keytool '-J-Duser.language=en' -genkeypair -keystore $key -storepass:file $storePassword -keypass:file $keyPassword `
    -alias crystalmix -keyalg RSA -keysize 3072 -validity 10000 -dname 'CN=CrystalMix, O=CrystalMix, C=CN' -storetype PKCS12
if($LASTEXITCODE -ne 0){throw 'Key generation failed. Inspect existing files before retrying.'}
'Signing identity generated. Back up the destination privately; never publish its contents.'
