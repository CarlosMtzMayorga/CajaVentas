#!/bin/zsh
# Reinicia la base de datos a su estado inicial (seed data)
export PATH="$HOME/.dotnet:$PATH"
cd "$(dirname "$0")"
rm -f src/CajaVenta.db
echo "✓ Base de datos eliminada"
echo "  La próxima ejecución recreará usuarios y productos de ejemplo"