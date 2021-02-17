import os 
from pathlib import Path

# simple function to cleanup test zip files 
def clean_zip_files():
    for zip_file in Path('.').glob('test-*.zip'):
        if(zip_file):
            os.remove(zip_file)
            print('files cleaned')
        else:
            print('no files to remove')

clean_zip_files()