import os
import shutil
import subprocess
import docker
from azure.storage.blob import BlockBlobService

# Set up directories
if os.path.exists('backup-data'):
    shutil.rmtree('backup-data')
os.makedirs(os.path.join('backup-data', 'content-cache'))
os.makedirs(os.path.join('backup-data', 'mssql'))

# Back up ees-mssql docker container db
client = docker.from_env()
container = client.containers.get('ees-mssql')
backup_cmds = [
    'rm -rf /tmp/*',
    '/opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P Your_Password123 -Q "BACKUP DATABASE [content] TO DISK = N\'/tmp/content.bak\' WITH FORMAT, NAME = \'content-full\', SKIP, NOREWIND, NOUNLOAD, COMPRESSION, STATS = 10"',
    '/opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P Your_Password123 -Q "BACKUP LOG [content] TO DISK = N\'/tmp/content_LogBackup.bak\' WITH NOFORMAT, NOINIT, NAME = N\'content_LogBackup\', NOSKIP, NOREWIND, NOUNLOAD, STATS = 5"',
    '/opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P Your_Password123 -Q "BACKUP DATABASE [statistics] TO DISK = N\'/tmp/statistics.bak\' WITH FORMAT, NAME = \'statistics-full\', SKIP, NOREWIND, NOUNLOAD, COMPRESSION, STATS = 10"',
    '/opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P Your_Password123 -Q "BACKUP LOG [statistics] TO DISK = N\'/tmp/statistics_LogBackup.bak\' WITH NOFORMAT, NOINIT, NAME = N\'statistics_LogBackup\', NOSKIP, NOREWIND, NOUNLOAD, STATS = 5"'
]
for cmd in backup_cmds:
    print(container.exec_run(cmd))

copy_from_container_cmds = [
    f'docker cp ees-mssql:/tmp/content.bak {os.path.join(os.getcwd(), "backup-data", "mssql")}',
    f'docker cp ees-mssql:/tmp/content_LogBackup.bak {os.path.join(os.getcwd(), "backup-data", "mssql")}',
    f'docker cp ees-mssql:/tmp/statistics.bak {os.path.join(os.getcwd(), "backup-data", "mssql")}',
    f'docker cp ees-mssql:/tmp/statistics_LogBackup.bak {os.path.join(os.getcwd(), "backup-data", "mssql")}'
]
for cmd in copy_from_container_cmds:
    print(subprocess.run(cmd.split()))

# Backup cache blob container to backup-data/content-cache
block_blob_service = BlockBlobService(is_emulated=True)
generator = block_blob_service.list_blobs('cache')
for blob in generator:
    if '/' in blob.name:
        head, tail = os.path.split(blob.name)
        os.makedirs(os.path.join(os.getcwd(), 'backup-data', 'content-cache', head), exist_ok=True)
        block_blob_service.get_blob_to_path('cache', blob.name, os.path.join(os.getcwd(), 'backup-data', 'content-cache', head, tail))
    else:
        block_blob_service.get_blob_to_path('cache', blob.name, os.path.join(os.getcwd(), 'backup-data', 'content-cache', blob.name))

