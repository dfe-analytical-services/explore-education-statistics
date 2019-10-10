import os
import docker
import subprocess
from azure.storage.blob import BlockBlobService

# Restore ees-mssql docker container db
client = docker.from_env()
container = client.containers.get('ees-mssql')
print(container.exec_run('rm -r /tmp/*'))
copy_to_container_cmds = [
    f'docker cp {os.path.join(os.getcwd(), "backup-data", "mssql", "content.bak")} ees-mssql:/tmp/',
    f'docker cp {os.path.join(os.getcwd(), "backup-data", "mssql", "content_LogBackup.bak")} ees-mssql:/tmp/',
    f'docker cp {os.path.join(os.getcwd(), "backup-data", "mssql", "statistics.bak")} ees-mssql:/tmp/',
    f'docker cp {os.path.join(os.getcwd(), "backup-data", "mssql", "statistics_LogBackup.bak")} ees-mssql:/tmp/'
]
for cmd in copy_to_container_cmds:
    print(subprocess.run(cmd.split()))

restore_cmds = [
    '/opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P Your_Password123 -Q "ALTER DATABASE [content] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; RESTORE DATABASE [content] FROM DISK = N\'/tmp/content.bak\' WITH FILE = 1, NOUNLOAD, REPLACE, NORECOVERY, STATS = 5"',
    '/opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P Your_Password123 -Q "RESTORE LOG [content] FROM DISK = N\'/tmp/content_LogBackup.bak\'"',
    '/opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P Your_Password123 -Q "ALTER DATABASE [content] SET MULTI_USER"',
    '/opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P Your_Password123 -Q "ALTER DATABASE [statistics] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; RESTORE DATABASE [statistics] FROM DISK = N\'/tmp/statistics.bak\' WITH FILE = 1, NOUNLOAD, REPLACE, NORECOVERY, STATS = 5"',
    '/opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P Your_Password123 -Q "RESTORE LOG [statistics] FROM DISK = N\'/tmp/statistics_LogBackup.bak\'"',
    '/opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P Your_Password123 -Q "ALTER DATABASE [statistics] SET MULTI_USER"'
]
for cmd in restore_cmds:
    print(container.exec_run(cmd))

# Delete all files in cache blob container
block_blob_service = BlockBlobService(is_emulated=True)
generator = block_blob_service.list_blobs('cache')
for blob in generator:
    block_blob_service.delete_blob('cache', blob.name)

# Restore all files to cache blob container
for (dirpath, _, filenames) in os.walk(os.path.join(os.getcwd(), 'backup-data', 'content-cache')):
    for filename in filenames:
        file_path = os.path.join(dirpath, filename)
        blob_name = os.path.join(dirpath, filename).split(f'content-cache{os.sep}', 1)[1]
        block_blob_service.create_blob_from_path('cache', blob_name, file_path)
