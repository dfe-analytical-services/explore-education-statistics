import csv
import os

from azure.core.exceptions import ResourceExistsError
from azure.storage.blob import BlobServiceClient
from tests.libs.logger import get_logger

logger = get_logger(__name__)

seed_data_csv_filepath = "tests/seed_data/seed_data_emulator_files.csv"
unzipped_seed_data_folderpath = "tests/files/.unzipped-seed-data-files"


class ReleaseFile:
    def __init__(self, release_id, file_id, filename=""):
        self.release_id = release_id
        self.file_id = file_id
        self.filename = filename
        self.content_type = "text/csv"

    def path(self):
        return f"{self.release_id}/data/{self.file_id}"


class ReleaseFilesGenerator(object):
    def __init__(self):
        # Instantiate a new ContainerClient for the emulator
        self.blob_service_client = BlobServiceClient.from_connection_string(
            "DefaultEndpointsProtocol=http;"
            + "AccountName=devstoreaccount1;"
            + "AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;"
            + "BlobEndpoint=http://data-storage:10000/devstoreaccount1"
        )

        self.data_files = []
        self.metadata_files = []

        if not os.path.exists(seed_data_csv_filepath):
            logger.warn("Unable to locate seed data csv file for generating local storage file uploads - skipping")
            return

        with open(seed_data_csv_filepath, "r") as seed_data_csv_file:
            csv_reader = csv.reader(seed_data_csv_file, delimiter=",")
            next(csv_reader)
            for row in csv_reader:
                file_type = row[3]
                if "Data" == file_type:
                    self.data_files.append(ReleaseFile(row[0], row[1], row[2]))
                else:
                    self.metadata_files.append(ReleaseFile(row[0], row[1], row[2]))

    def create_public_release_files(self):
        try:
            container_client = self.blob_service_client.create_container("downloads")
        except ResourceExistsError:
            container_client = self.blob_service_client.get_container_client("downloads")
        except Exception as e:
            logger.error('Unexpected exception creating "downloads" container: ', e)

        assert container_client is not None

        self.upload_files(container_client, self.data_files)

    def create_private_release_files(self):
        try:
            container_client = self.blob_service_client.create_container("releases")
        except ResourceExistsError:
            container_client = self.blob_service_client.get_container_client("releases")
        except Exception as e:
            logger.error('Unexpected exception creating "releases" container: ', e)

        assert container_client is not None

        self.upload_files(container_client, self.data_files)
        self.upload_files(container_client, self.metadata_files)

    @staticmethod
    def upload_files(container_client, files):
        files_by_filename = {}
        for file in files:
            collection = files_by_filename.setdefault(file.filename, [])
            collection.append(file)

        for filename in files_by_filename:
            seed_data_filepath = f"{unzipped_seed_data_folderpath}/{filename}"

            if os.path.exists(seed_data_filepath):
                with open(seed_data_filepath, "r") as seed_file:
                    data = seed_file.read()
            else:
                logger.warn(
                    f"Unable to locate unzipped seed data file at location {seed_data_filepath} - "
                    f"creating dummy file contents instead"
                )
                data = b"abcd" * 128

            for file in files_by_filename[filename]:
                blob_path = file.path()
                blob_client = container_client.get_blob_client(blob_path)
                if not blob_client.exists():
                    logger.info(f"Uploading seed data file {filename} to local storage path {blob_path}")
                    try:
                        blob_client.upload_blob(data, blob_type="BlockBlob", content_type=file.content_type)
                    except ResourceExistsError:
                        pass  # file already exists, so no action required
                    except Exception as e:
                        logger.error("Unexpected exception when uploading file: ", e)


if __name__ == "__main__":
    generator = ReleaseFilesGenerator()
    generator.create_public_release_files()
    generator.create_private_release_files()
