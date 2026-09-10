#!/bin/zsh
# Script de arranque para CajaVenta POS
export PATH="$HOME/.dotnet:$PATH"
cd "$(dirname "$0")"
dotnet run --project src/CajaVenta.Web