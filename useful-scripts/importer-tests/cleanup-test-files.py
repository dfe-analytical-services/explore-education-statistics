import os 
from pathlib import Path

# simple function to cleanup test zip files 
def clean_zip_files():
    for ZIP_FILE in Path('.').glob('test-*.zip'):
        if(ZIP_FILE):
            os.remove(ZIP_FILE)
            print('files cleaned')
        else:
            print('no files to remove')

clean_zip_files()