# User-configurable variables
$serviceName = "eRecipesService" # Change this to the name of your service
$password = "createYourPasswordHere" # Set a password for your PFX file
$pfxFilePath = "eRecipesService.pfx" # Specify the path for the PFX file  
$outputCerFile = "eRecipesServiceCert.cer"  # test this
$ipAddress = "0.0.0.0:5757" # Set your service's IP address

# Get all certificates in the LocalMachine\My store matching the subject name pattern
$certsToDelete = Get-ChildItem -Path Cert:\LocalMachine\My | Where-Object { $_.Subject -eq "CN=$serviceName" }

# Delete any certs with the same name... Be sure this is what you want
if ($certsToDelete -ne $null) {
    $certsToDelete | Where-Object { $_.Subject -eq "CN=$serviceName" } | ForEach-Object {
        # Display the certificate being removed (optional, for verification)
        Write-Host "Deleting certificate: $($_.Subject)"
        
        # Remove the certificate
        Remove-Item $_.PSPath
    }
    Write-Host "All matching certificates have been deleted."
} else {
    Write-Host "No certificates found matching the criteria."
}

# Convert password to secure string
$pwd = ConvertTo-SecureString -String $password -Force -AsPlainText

# Create the self-signed certificate
$cert = New-SelfSignedCertificate -DnsName $serviceName -CertStoreLocation Cert:\LocalMachine\My -NotAfter (Get-Date).AddYears(5)

# Export to PFX file (optional)
# Export-PfxCertificate -Cert $cert -FilePath $pfxFilePath -Password $pwd

# Import to Trusted Root Certification Authorities
# Import-Certificate -FilePath $pfxFilePath -CertStoreLocation Cert:\LocalMachine\Root -Password $pwd

# Prepare the certificate thumbprint and GUID for binding
$certHash = $cert.Thumbprint
$guid = [guid]::NewGuid().ToString()

# Unbind the certificate to the IP address and port
netsh http delete sslcert ipport=$ipAddress

# Bind the certificate to the IP address and port
netsh http add sslcert ipport=$ipAddress certhash=$certHash appid=`{$guid`} certstorename=MY

# Retrieve the certificate from the Local Machine store and select the first one if there are multiple
$cert = Get-ChildItem -Path Cert:\LocalMachine\My | Where-Object { $_.Subject -like "*$serviceName*" } | Select-Object -First 1

# Check if a certificate was found
if ($cert -ne $null) {
    # Export the certificate to a .cer file
    Export-Certificate -Cert $cert -FilePath $outputCerFile
    Write-Host "Certificate exported to $outputCerFile"
} else {
    Write-Host "Certificate with subject name '$serviceName' not found."
}

certutil -addstore TrustedPeople "$outputCerFile"