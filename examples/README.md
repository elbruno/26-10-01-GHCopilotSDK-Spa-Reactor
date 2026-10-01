# Hello, GitHub Copilot SDK

Ejemplos mínimos para comparar la misma primera sesión en varios lenguajes.
Están adaptados del
[workshop oficial](https://github.com/github/copilot-sdk-workshop/tree/716bdaaf629817606873b4e22d551e900985c92c)
y fijan GitHub Copilot SDK `1.0.11`.

Estos archivos se muestran brevemente durante la slide 4. La demo principal
continúa en .NET con `AccessibilityDemo.csproj`.

## Python

```powershell
Set-Location .\python
python -m venv .venv
.\.venv\Scripts\python.exe -m pip install -e .
.\.venv\Scripts\python.exe .\main.py
```

## Go

```powershell
Set-Location .\go
go mod download
go run .
```

## TypeScript

```powershell
Set-Location .\typescript
npm ci
npm start
```

Cada ejemplo mantiene un cliente, crea una sesión, envía un mensaje y libera
los recursos. No incluyen tools ni permisos: su objetivo es mostrar que el
modelo de programación es equivalente entre lenguajes.
