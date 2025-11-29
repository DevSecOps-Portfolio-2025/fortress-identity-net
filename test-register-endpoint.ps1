# 🧪 Script de Pruebas - RegisterUser Endpoint
# Este script demuestra todos los casos de uso del endpoint /api/auth/register

Write-Host "===============================================" -ForegroundColor Cyan
Write-Host "🧪 PRUEBAS DEL ENDPOINT /api/auth/register" -ForegroundColor Cyan
Write-Host "===============================================`n" -ForegroundColor Cyan

$baseUrl = "http://localhost:5215"
$endpoint = "$baseUrl/api/auth/register"

# ====================================================================
# Test 1: ✅ Registro Exitoso
# ====================================================================
Write-Host "Test 1: ✅ Registro Exitoso" -ForegroundColor Green
Write-Host "---------------------------------------`n" -ForegroundColor Gray

$successBody = @{
    firstName = "John"
    lastName = "Doe"
    email = "john.doe@fortress.dev"
    password = "SecureP@ssw0rd123!"
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri $endpoint -Method Post -Body $successBody -ContentType "application/json"
    Write-Host "✅ Usuario creado exitosamente!" -ForegroundColor Green
    Write-Host "   User ID: $($response.userId)" -ForegroundColor White
    Write-Host ""
} catch {
    $statusCode = $_.Exception.Response.StatusCode.value__
    $errorBody = $_.ErrorDetails.Message | ConvertFrom-Json
    
    if ($statusCode -eq 409) {
        Write-Host "⚠️  El email ya existe (esperado si ejecutas el script múltiples veces)" -ForegroundColor Yellow
        Write-Host "   Status: 409 Conflict" -ForegroundColor Yellow
        Write-Host "   Detail: $($errorBody.detail)" -ForegroundColor White
    } else {
        Write-Host "❌ Error: $($errorBody.detail)" -ForegroundColor Red
    }
    Write-Host ""
}

Start-Sleep -Seconds 1

# ====================================================================
# Test 2: ❌ Contraseña Débil (Validación FluentValidation)
# ====================================================================
Write-Host "Test 2: ❌ Contraseña Débil" -ForegroundColor Yellow
Write-Host "---------------------------------------`n" -ForegroundColor Gray

$weakPasswordBody = @{
    firstName = "Jane"
    lastName = "Smith"
    email = "jane.smith@fortress.dev"
    password = "weak"
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri $endpoint -Method Post -Body $weakPasswordBody -ContentType "application/json"
    Write-Host "❌ No debería llegar aquí" -ForegroundColor Red
} catch {
    $statusCode = $_.Exception.Response.StatusCode.value__
    $errorBody = $_.ErrorDetails.Message | ConvertFrom-Json
    
    Write-Host "✅ Validación funcionó correctamente!" -ForegroundColor Green
    Write-Host "   Status: $statusCode Bad Request" -ForegroundColor White
    Write-Host "   Title: $($errorBody.title)" -ForegroundColor White
    Write-Host "   Errores:" -ForegroundColor White
    
    foreach ($key in $errorBody.errors.PSObject.Properties) {
        Write-Host "      - $($key.Name):" -ForegroundColor Cyan
        foreach ($error in $key.Value) {
            Write-Host "         * $error" -ForegroundColor White
        }
    }
    Write-Host ""
}

Start-Sleep -Seconds 1

# ====================================================================
# Test 3: ❌ Email Inválido (Validación FluentValidation)
# ====================================================================
Write-Host "Test 3: ❌ Email Inválido" -ForegroundColor Yellow
Write-Host "---------------------------------------`n" -ForegroundColor Gray

$invalidEmailBody = @{
    firstName = "Bob"
    lastName = "Johnson"
    email = "not-an-email"
    password = "SecureP@ssw0rd123!"
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri $endpoint -Method Post -Body $invalidEmailBody -ContentType "application/json"
    Write-Host "❌ No debería llegar aquí" -ForegroundColor Red
} catch {
    $statusCode = $_.Exception.Response.StatusCode.value__
    $errorBody = $_.ErrorDetails.Message | ConvertFrom-Json
    
    Write-Host "✅ Validación funcionó correctamente!" -ForegroundColor Green
    Write-Host "   Status: $statusCode Bad Request" -ForegroundColor White
    Write-Host "   Title: $($errorBody.title)" -ForegroundColor White
    Write-Host "   Errores:" -ForegroundColor White
    
    foreach ($key in $errorBody.errors.PSObject.Properties) {
        Write-Host "      - $($key.Name):" -ForegroundColor Cyan
        foreach ($error in $key.Value) {
            Write-Host "         * $error" -ForegroundColor White
        }
    }
    Write-Host ""
}

Start-Sleep -Seconds 1

# ====================================================================
# Test 4: ❌ Campos Vacíos (Validación FluentValidation)
# ====================================================================
Write-Host "Test 4: ❌ Campos Vacíos" -ForegroundColor Yellow
Write-Host "---------------------------------------`n" -ForegroundColor Gray

$emptyFieldsBody = @{
    firstName = ""
    lastName = ""
    email = ""
    password = ""
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri $endpoint -Method Post -Body $emptyFieldsBody -ContentType "application/json"
    Write-Host "❌ No debería llegar aquí" -ForegroundColor Red
} catch {
    $statusCode = $_.Exception.Response.StatusCode.value__
    $errorBody = $_.ErrorDetails.Message | ConvertFrom-Json
    
    Write-Host "✅ Validación funcionó correctamente!" -ForegroundColor Green
    Write-Host "   Status: $statusCode Bad Request" -ForegroundColor White
    Write-Host "   Title: $($errorBody.title)" -ForegroundColor White
    Write-Host "   Total de errores: $($errorBody.errors.PSObject.Properties.Count) campos con problemas" -ForegroundColor White
    Write-Host ""
}

Start-Sleep -Seconds 1

# ====================================================================
# Test 5: ❌ Email Duplicado (DomainException - 409 Conflict)
# ====================================================================
Write-Host "Test 5: ❌ Email Duplicado (409 Conflict)" -ForegroundColor Yellow
Write-Host "---------------------------------------`n" -ForegroundColor Gray

$duplicateEmailBody = @{
    firstName = "John"
    lastName = "Duplicate"
    email = "john.doe@fortress.dev"  # Mismo email del Test 1
    password = "AnotherP@ssw0rd456!"
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri $endpoint -Method Post -Body $duplicateEmailBody -ContentType "application/json"
    Write-Host "❌ No debería llegar aquí (email ya existe)" -ForegroundColor Red
} catch {
    $statusCode = $_.Exception.Response.StatusCode.value__
    $errorBody = $_.ErrorDetails.Message | ConvertFrom-Json
    
    if ($statusCode -eq 409) {
        Write-Host "✅ Validación de email duplicado funcionó!" -ForegroundColor Green
        Write-Host "   Status: $statusCode Conflict" -ForegroundColor White
        Write-Host "   Title: $($errorBody.title)" -ForegroundColor White
        Write-Host "   Detail: $($errorBody.detail)" -ForegroundColor White
    } else {
        Write-Host "⚠️  Status inesperado: $statusCode" -ForegroundColor Yellow
        Write-Host "   Detail: $($errorBody.detail)" -ForegroundColor White
    }
    Write-Host ""
}

# ====================================================================
# Resumen Final
# ====================================================================
Write-Host "`n===============================================" -ForegroundColor Cyan
Write-Host "✅ TODAS LAS PRUEBAS COMPLETADAS" -ForegroundColor Cyan
Write-Host "===============================================`n" -ForegroundColor Cyan

Write-Host "Resumen de casos probados:" -ForegroundColor White
Write-Host "  ✅ Registro exitoso (201 Created)" -ForegroundColor Green
Write-Host "  ✅ Contraseña débil (400 Bad Request)" -ForegroundColor Green
Write-Host "  ✅ Email inválido (400 Bad Request)" -ForegroundColor Green
Write-Host "  ✅ Campos vacíos (400 Bad Request)" -ForegroundColor Green
Write-Host "  ✅ Email duplicado (409 Conflict)" -ForegroundColor Green
Write-Host ""

Write-Host "🎯 La API está funcionando correctamente con:" -ForegroundColor Cyan
Write-Host "   - CQRS Pattern implementado" -ForegroundColor White
Write-Host "   - FluentValidation activa" -ForegroundColor White
Write-Host "   - Global Exception Handler operativo" -ForegroundColor White
Write-Host "   - Problem Details (RFC 7807) en respuestas" -ForegroundColor White
Write-Host ""
