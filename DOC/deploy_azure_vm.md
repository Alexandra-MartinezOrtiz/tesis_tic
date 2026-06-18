# Despliegue en Azure VM

Este repo puede correr en una VM de Azure con Docker Compose. El frontend queda publicado en el puerto `4200` y las llamadas `/api` se enrutan internamente al gateway por Nginx, por eso no hace falta exponer el puerto `5000` al publico.

## Primera instalacion manual en la VM

```bash
mkdir -p ~/proyecto
cd ~/proyecto
git clone git@github.com:Alexandra-MartinezOrtiz/tesis_tic.git
cd tesis_tic
bash deploy/azure-vm.sh
```

Verifica:

```bash
docker compose ps
docker compose logs --tail=100
```

Abre en Azure solo el puerto necesario para la web:

- Puerto: `4200`
- Protocolo: `TCP`
- Accion: `Allow`
- Origen: `Any`
- Nombre: `Allow-Frontend-4200`

No abras `5432` para Postgres. El compose lo publica solo en `127.0.0.1` para uso local de la VM.

## Despliegue automatico desde GitHub Actions

El workflow `.github/workflows/deploy-azure-vm.yml` despliega en cada push a `main` o manualmente desde `Actions`. Si faltan secretos, el workflow se omite sin desplegar.

Configura estos secretos en GitHub:

- `AZURE_VM_HOST`: IP publica o DNS de la VM, por ejemplo `20.106.101.116`
- `AZURE_VM_USER`: usuario SSH de la VM
- `AZURE_VM_SSH_KEY`: llave privada SSH que entra a la VM
- `AZURE_VM_PORT`: opcional, usa `22` si no existe

La VM tambien debe tener acceso SSH al repo de GitHub, porque dentro de la VM se ejecuta:

```bash
git clone git@github.com:Alexandra-MartinezOrtiz/tesis_tic.git
```

## Actualizar manualmente

```bash
cd ~/proyecto/tesis_tic
bash deploy/azure-vm.sh
```
