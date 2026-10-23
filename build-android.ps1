param(
    [string]$SdkDirectory = (Join-Path $PSScriptRoot '.tools\android-sdk'),
    [string]$JavaDirectory = (Join-Path $PSScriptRoot '.tools\android-jdk'),
    [string]$KeyStore = (Join-Path $PSScriptRoot '.signing\crystalmix-android.keystore'),
    [string]$PasswordFile = (Join-Path $PSScriptRoot '.signing\android-password.txt'),
    [string]$KeyPasswordFile = (Join-Path $PSScriptRoot '.signing\android-key-password.txt')
)
$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot
$dotnet = if(Test-Path '.tools\dotnet\dotnet.exe'){Join-Path $PWD '.tools\dotnet\dotnet.exe'}else{'dotnet'}
$env:DOTNET_CLI_HOME=Join-Path $PWD '.tools\dotnet-home'
$env:NUGET_PACKAGES=Join-Path $PWD '.tools\packages'
$env:JAVA_HOME=$JavaDirectory
$env:ANDROID_USER_HOME=Join-Path $PWD '.tools\android-user'
$env:ANDROID_AVD_HOME=Join-Path $PWD '.tools\android-avd'
foreach($path in @($SdkDirectory,$JavaDirectory,$KeyStore,$PasswordFile,$KeyPasswordFile)){
    if(-not(Test-Path -LiteralPath $path)){throw "Required build dependency missing: $path. See ANDROID.md."}
}
& $dotnet publish CrystalMix.Android -c Release "-p:AndroidSdkDirectory=$SdkDirectory" "-p:JavaSdkDirectory=$JavaDirectory" `
    -p:AndroidKeyStore=true "-p:AndroidSigningKeyStore=$KeyStore" -p:AndroidSigningKeyAlias=crystalmix `
    "-p:AndroidSigningKeyPass=file:$KeyPasswordFile" "-p:AndroidSigningStorePass=file:$PasswordFile" --verbosity quiet
if($LASTEXITCODE -ne 0){throw 'Android publish failed.'}
$apk=Get-ChildItem 'CrystalMix.Android\bin\Release\net8.0-android\publish' -Filter '*.apk' | Where-Object Name -NotLike '*-Signed.apk' | Select-Object -First 1
if(-not $apk){throw 'APK not found.'}
New-Item -ItemType Directory -Force dist | Out-Null
[xml]$project=Get-Content CrystalMix.Android/CrystalMix.Android.csproj
$version=$project.Project.PropertyGroup.ApplicationDisplayVersion
$apkName="CrystalMix-Android-$version.apk"
$destination=Join-Path $PWD ("dist\" + $apkName)
$signer=Get-ChildItem (Join-Path $SdkDirectory 'build-tools') -Recurse -Filter apksigner.jar | Sort-Object FullName -Descending | Select-Object -First 1
$zipalign=Join-Path (Split-Path (Split-Path $signer.FullName)) 'zipalign.exe'
$aligned=Join-Path $PWD 'dist\android-aligned.apk'
& $zipalign -f -p 4 $apk.FullName $aligned
if($LASTEXITCODE -ne 0){throw 'APK alignment failed.'}
$java=Join-Path $JavaDirectory 'bin\java.exe'
& $java -jar $signer.FullName sign --ks $KeyStore --ks-key-alias crystalmix --ks-pass "file:$PasswordFile" --key-pass "file:$KeyPasswordFile" --out $destination $aligned
if($LASTEXITCODE -ne 0){throw 'APK release signing failed.'}
& $java -jar $signer.FullName verify --verbose --print-certs $destination
if($LASTEXITCODE -ne 0){throw 'APK signature verification failed.'}
$hash=(Get-FileHash $destination).Hash.ToLowerInvariant()
"$hash  $apkName" | Set-Content dist/ANDROID-SHA256SUMS.txt -Encoding ascii
Get-Item $destination | Select-Object Name,Length
'Android APK built and signature verified.'
