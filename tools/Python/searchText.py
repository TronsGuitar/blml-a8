import os
import re
from concurrent.futures import ThreadPoolExecutor

def search_file(filepath, keyword):
    try:
        with open(filepath, 'r', errors='ignore') as file:
            if re.search(keyword, file.read()):
                print(f'Keyword found in: {filepath}')
    except Exception as e:
        print(f'Error reading {filepath}: {e}')

def search_files(root_folder, keyword):
    with ThreadPoolExecutor() as executor:
        for foldername, subfolders, filenames in os.walk(root_folder):
            for filename in filenames:
                filepath = os.path.join(foldername, filename)
                executor.submit(search_file, filepath, keyword)

search_files(r'C:\path\to\search', r'keyword')
