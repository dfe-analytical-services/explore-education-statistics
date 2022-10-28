from abc import ABC, abstractmethod

from azure.core.exceptions import ResourceExistsError
from azure.storage.blob import BlobServiceClient
from tests.libs.logger import get_logger

logger = get_logger(__name__)


class File(ABC):
    def __init__(self, release, id, content_type):
        self.release = release
        self.id = id
        self.content_type = content_type

    @abstractmethod
    def path(self):
        pass


class DataFile(File):
    def __init__(self, release, id):
        super(DataFile, self).__init__(release, id, "text/csv")

    def path(self):
        return f"{self.release.id}/data/{self.id}"


class MetadataFile(File):
    def __init__(self, release, id):
        super(MetadataFile, self).__init__(release, id, "text/csv")

    def path(self):
        return f"{self.release.id}/data/{self.id}"


class Release:
    def __init__(self, id):
        self.id = id


class ReleaseFilesGenerator(object):
    def __init__(self):

        # Instantiate a new ContainerClient for the emulator
        self.blob_service_client = BlobServiceClient.from_connection_string(
            "DefaultEndpointsProtocol=http;"
            + "AccountName=devstoreaccount1;"
            + "AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;"
            + "BlobEndpoint=http://data-storage:10000/devstoreaccount1"
        )

        exclusions_release = Release("e7774a74-1f62-4b76-b9b5-84f14dac7278")
        pupil_absence_release = Release("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5")

        self.data_files = [
            # Exclusions
            DataFile(exclusions_release, "0ddf5b95-874b-4bcd-6a8f-08d7ec2d8168"),
            DataFile(exclusions_release, "a5eb34a2-4615-4ee4-6a90-08d7ec2d8168"),
            DataFile(exclusions_release, "0d8b6e60-7b92-4f4d-6a91-08d7ec2d8168"),
            DataFile(exclusions_release, "25b691c9-4d07-437f-6a93-08d7ec2d8168"),
            DataFile(exclusions_release, "f5c56849-f634-4fda-6a92-08d7ec2d8168"),
            DataFile(exclusions_release, "f0538c07-fc64-423b-6a94-08d7ec2d8168"),
            # Pupil absence
            DataFile(pupil_absence_release, "15c05193-1b4a-4acb-6a81-08d7ec2d8168"),
            DataFile(pupil_absence_release, "cf0e3cb5-ea9c-45b1-6a83-08d7ec2d8168"),
            DataFile(pupil_absence_release, "d4d72886-8921-426a-6a82-08d7ec2d8168"),
            DataFile(pupil_absence_release, "bcf86eb2-4244-41aa-6a84-08d7ec2d8168"),
            DataFile(pupil_absence_release, "c1239606-300a-4782-6a85-08d7ec2d8168"),
            DataFile(pupil_absence_release, "c8141534-d706-49d0-6a86-08d7ec2d8168"),
            DataFile(pupil_absence_release, "28199189-f0b9-44f2-6a87-08d7ec2d8168"),
        ]

        self.metadata_files = [
            # Exclusions
            MetadataFile(exclusions_release, "2a90141f-3331-4f00-6a98-08d7ec2d8168"),
            MetadataFile(exclusions_release, "04b45150-0492-4b8b-6a95-08d7ec2d8168"),
            MetadataFile(exclusions_release, "7af9b369-1487-41f4-6a96-08d7ec2d8168"),
            MetadataFile(exclusions_release, "8a7d0775-8b51-47e3-6a99-08d7ec2d8168"),
            MetadataFile(exclusions_release, "5a22f935-6d50-4d76-6a97-08d7ec2d8168"),
            MetadataFile(exclusions_release, "9a917320-891c-4a00-6a9a-08d7ec2d8168"),
            # Pupil absence
            MetadataFile(pupil_absence_release, "be141b27-0143-41b0-6a88-08d7ec2d8168"),
            MetadataFile(pupil_absence_release, "c2a67d23-b135-4d84-6a8a-08d7ec2d8168"),
            MetadataFile(pupil_absence_release, "5f355f7c-a6ec-481a-6a89-08d7ec2d8168"),
            MetadataFile(pupil_absence_release, "f06041a4-e468-44df-6a8b-08d7ec2d8168"),
            MetadataFile(pupil_absence_release, "ec069435-bdc0-4c2d-6a8c-08d7ec2d8168"),
            MetadataFile(pupil_absence_release, "f9c1eecc-e0a0-40cd-6a8d-08d7ec2d8168"),
            MetadataFile(pupil_absence_release, "36c7efeb-ffe9-4444-6a8e-08d7ec2d8168"),
        ]

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
        data = b"abcd" * 128
        for file in files:
            path = file.path()

            blob_client = container_client.get_blob_client(path)
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
