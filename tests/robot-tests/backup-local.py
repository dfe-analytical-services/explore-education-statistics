import os
import shutil
import subprocess
import docker
from azure.storage.blob import BlockBlobService

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
    '''/opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P Your_Password123 -Q "BACKUP DATABASE [content] TO DISK = N'/tmp/content.bak' WITH NOFORMAT, INIT, NAME = 'content-full', NOSKIP, REWIND, NOUNLOAD, COMPRESSION, STATS = 10"''',
    #'''/opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P Your_Password123 -Q "declare @backupSetIdContent as int select @backupSetIdContent = position from msdb..backupset where database_name=N'content' and backup_set_id=(select max(backup_set_id) from msdb..backupset where database_name=N'content' ) if @backupSetIdContent is null begin raiserror(N'Verify failed. Backup information for database ''content'' not found.', 16, 1) end RESTORE VERIFYONLY FROM  DISK = N'/tmp/content.bak' WITH  FILE = @backupSetIdContent,  NOUNLOAD"'''
    '''/opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P Your_Password123 -Q "BACKUP DATABASE [statistics] TO DISK = N'/tmp/statistics.bak' WITH NOFORMAT, INIT, NAME = 'statistics-full', NOSKIP, REWIND, NOUNLOAD, COMPRESSION, STATS = 10"''',
    #'''/opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P Your_Password123 -Q "declare @backupSetIdStatistics as int select @backupSetIdStatistics = position from msdb..backupset where database_name=N'statistics' and backup_set_id=(select max(backup_set_id) from msdb..backupset where database_name=N'statistics' ) if @backupSetIdStatistics is null begin raiserror(N'Verify failed. Backup information for database ''statistics'' not found.', 16, 1) end RESTORE VERIFYONLY FROM  DISK = N'/tmp/statistics.bak' WITH  FILE = @backupSetIdStatistics,  NOUNLOAD"'''
]
for cmd in backup_cmds:
    print(container.exec_run(cmd))

copy_from_container_cmds = [
    f'docker cp ees-mssql:/tmp/content.bak {os.path.join(os.getcwd(), "backup-data", "mssql")}',
    f'docker cp ees-mssql:/tmp/statistics.bak {os.path.join(os.getcwd(), "backup-data", "mssql")}',
]
import time
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
        block_blob_service.get_blob_to_path('cache', blob.name, os.path.join(os.getcwd(), 'backup-data', 'content-cache', head, tail))
    else:
        block_blob_service.get_blob_to_path('cache', blob.name, os.path.join(os.getcwd(), 'backup-data', 'content-cache', blob.name))

# Backup downloads blob container to backup-data/releases
os.makedirs(os.path.join('backup-data', 'releases'))
block_blob_service = BlockBlobService(is_emulated=True)
generator = block_blob_service.list_blobs('releases')
for blob in generator:
    if '/' in blob.name:
        head, tail = os.path.split(blob.name)
        os.makedirs(os.path.join(os.getcwd(), 'backup-data', 'releases', head), exist_ok=True)
        block_blob_service.get_blob_to_path('releases', blob.name, os.path.join(os.getcwd(), 'backup-data', 'releases', head, tail))
    else:
        block_blob_service.get_blob_to_path('releases', blob.name, os.path.join(os.getcwd(), 'backup-data', 'releases', blob.name))

