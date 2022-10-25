import time
import os
import shutil
import subprocess
import docker
import json
from azure.storage.blob import BlockBlobService

# Variables used by AzCopy
AzCopy = 'C:\\Program Files (x86)\\Microsoft SDKs\\Azure\\AzCopy\\AzCopy.exe'
storage_table_url = 'http://127.0.0.1:10002/devstoreaccount1'
storage_key = 'Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw=='

# Remove all previous backup files and directories in backup-data directory
if os.path.exists('backup-data'):
    shutil.rmtree('backup-data')

# Back up ees-mssql docker container db
os.makedirs(os.path.join('backup-data', 'mssql'))
client = docker.from_env()
container = client.containers.get('ees-mssql')
backup_cmds = [
    'rm -rf /tmp/',
    'mkdir tmp',
    '''/opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P Your_Password123 -Q "ALTER DATABASE [content] SET SINGLE_USER WITH ROLLBACK IMMEDIATE"''',
    '''/opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P Your_Password123 -Q "BACKUP DATABASE [content] TO DISK = N'/tmp/content.bak' WITH NOFORMAT, NOINIT, NAME = 'content-full', SKIP, NOREWIND, NOUNLOAD, COMPRESSION, STATS = 10"''',
    '''/opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P Your_Password123 -Q "ALTER DATABASE [content] SET MULTI_USER WITH ROLLBACK IMMEDIATE"''',
    '''/opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P Your_Password123 -Q "ALTER DATABASE [statistics] SET SINGLE_USER WITH ROLLBACK IMMEDIATE"''',
    '''/opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P Your_Password123 -Q "BACKUP DATABASE [statistics] TO DISK = N'/tmp/statistics.bak' WITH NOFORMAT, NOINIT, NAME = 'statistics-full', SKIP, NOREWIND, NOUNLOAD, COMPRESSION, STATS = 10"''',
    '''/opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P Your_Password123 -Q "ALTER DATABASE [statistics] SET MULTI_USER WITH ROLLBACK IMMEDIATE"''',
]
for cmd in backup_cmds:
    print(container.exec_run(cmd))

copy_from_container_cmds = [
    f'docker cp ees-mssql:/tmp/content.bak {os.path.join(os.getcwd(), "backup-data", "mssql")}',
    f'docker cp ees-mssql:/tmp/statistics.bak {os.path.join(os.getcwd(), "backup-data", "mssql")}',
]
for cmd in copy_from_container_cmds:
    print(subprocess.run(cmd.split()))
    time.sleep(1)

# Backup cache blob container to backup-data/content-cache
os.makedirs(os.path.join('backup-data', 'content-cache'))
block_blob_service = BlockBlobService(is_emulated=True)
generator = block_blob_service.list_blobs('cache')
for blob in generator:
    if '/' in blob.name:
        head, tail = os.path.split(blob.name)
        os.makedirs(os.path.join(os.getcwd(), 'backup-data', 'content-cache', head), exist_ok=True)
        block_blob_service.get_blob_to_path(
            'cache', blob.name, os.path.join(
                os.getcwd(), 'backup-data', 'content-cache', head, tail))
    else:
        block_blob_service.get_blob_to_path(
            'cache', blob.name, os.path.join(
                os.getcwd(), 'backup-data', 'content-cache', blob.name))

# Backup releases blob container to backup-data/releases
os.makedirs(os.path.join('backup-data', 'releases'))
block_blob_service = BlockBlobService(is_emulated=True)
generator = block_blob_service.list_blobs('releases')
for blob in generator:
    if '/' in blob.name:
        # Save file
        head, tail = os.path.split(blob.name)
        os.makedirs(os.path.join(os.getcwd(), 'backup-data', 'releases', head), exist_ok=True)
        blob_obj = block_blob_service.get_blob_to_path(
            'releases', blob.name, os.path.join(
                os.getcwd(), 'backup-data', 'releases', head, tail))
        # Save metadata
        with open(os.path.join(os.getcwd(), 'backup-data', 'releases', head, tail + '.metadata'), 'w') as file:
            file.write(json.dumps(blob_obj.metadata))
    else:
        # Save file
        blob_obj = block_blob_service.get_blob_to_path(
            'releases', blob.name, os.path.join(
                os.getcwd(), 'backup-data', 'releases', blob.name))
        # Save metadata
        with open(os.path.join(os.getcwd(), 'backup-data', 'releases', blob.name + '.metadata'), 'w') as file:
            file.write(blob_obj.metadata)

# Backup downloads blob container to backup-data/downloads
os.makedirs(os.path.join('backup-data', 'downloads'))
block_blob_service = BlockBlobService(is_emulated=True)
generator = block_blob_service.list_blobs('downloads')
for blob in generator:
    if '/' in blob.name:
        # Save file
        head, tail = os.path.split(blob.name)
        os.makedirs(os.path.join(os.getcwd(), 'backup-data', 'downloads', head), exist_ok=True)
        blob_obj = block_blob_service.get_blob_to_path(
            'downloads', blob.name, os.path.join(
                os.getcwd(), 'backup-data', 'downloads', head, tail))
        # Save metadata
        with open(os.path.join(os.getcwd(), 'backup-data', 'downloads', head, tail + '.metadata'), 'w') as file:
            file.write(json.dumps(blob_obj.metadata))
    else:
        # Save file
        blob_obj = block_blob_service.get_blob_to_path(
            'downloads', blob.name, os.path.join(
                os.getcwd(), 'backup-data', 'downloads', blob.name))
        # Save metadata
        with open(os.path.join(os.getcwd(), 'backup-data', 'downloads', blob.name + '.metadata'), 'w') as file:
            file.write(blob_obj.metadata)

# Backup imports table to backup-data/imports
os.makedirs(os.path.join('backup-data', 'imports'))
backup_table_cmd = [
    AzCopy,
    f'/Source:{storage_table_url}/imports',
    '/sourceType:table',
    f'/Dest:{os.path.join(os.getcwd(),"backup-data", "imports")}',
    f'/SourceKey:{storage_key}',
    '/Manifest:table-backup.manifest']
print(subprocess.run(backup_table_cmd))

# Backup ReleaseStatus table to backup-data/ReleaseStatus
os.makedirs(os.path.join('backup-data', 'ReleaseStatus'))
backup_table_cmd = [
    AzCopy,
    f'/Source:{storage_table_url}/ReleaseStatus',
    '/sourceType:table',
    f'/Dest:{os.path.join(os.getcwd(),"backup-data", "ReleaseStatus")}',
    f'/SourceKey:{storage_key}',
    '/Manifest:table-backup.manifest']
print(subprocess.run(backup_table_cmd))
