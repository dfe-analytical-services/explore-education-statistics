from abc import ABC, abstractmethod
from azure.core.exceptions import ResourceExistsError
from azure.storage.blob import BlobServiceClient
from datetime import datetime, timezone


class File(ABC):
    def __init__(self, release, id, name=None, filename=None):
        self.release = release
        self.id = id
        self.name = name
        # EES-1704 Temporary field used for legacy path
        self.filename = filename
        # Create a timestamp matching "yyyy-MM-ddTHH:mm:ss.mmmmmmmZ"
        self.published = datetime.now(
            timezone.utc).isoformat().replace("+00:00", "0Z")

    @abstractmethod
    def metadata(self):
        pass

    @abstractmethod
    def path(self):
        pass

    # EES-1704 Temporary methods in case still generating legacy file structure
    @abstractmethod
    def legacy_private_path(self):
        pass

    @abstractmethod
    def legacy_public_path(self):
        pass


class AncillaryFile(File):
    def metadata(self):
        return {
            "name": self.name,
            "releasedatetime": self.published
        }

    def path(self):
        return f"{self.release.id}/ancillary/{self.id}"

    def legacy_private_path(self):
        return f"{self.release.id}/ancillary/{self.id}"

    def legacy_public_path(self):
        return f"{self.release.publication_slug}/{self.release.slug}/ancillary/{self.id}"


class DataFile(File):
    def metadata(self):
        return {
            "userName": "ees-analyst1@education.gov.uk",
            "NumberOfRows": "1000",
            "name": self.name,
            "releasedatetime": self.published
        }

    def path(self):
        return f"{self.release.id}/data/{self.id}"

    def legacy_private_path(self):
        return f"{self.release.id}/data/{self.filename}"

    def legacy_public_path(self):
        return f"{self.release.publication_slug}/{self.release.slug}/data/{self.filename}"


class MetadataFile(File):
    def metadata(self):
        return {}

    def path(self):
        return f"{self.release.id}/data/{self.id}"

    def legacy_private_path(self):
        return f"{self.release.id}/data/{self.filename}"

    def legacy_public_path(self):
        return f"{self.release.publication_slug}/{self.release.slug}/data/{self.filename}"


class Release:
    def __init__(self, id, slug, publication_slug):
        self.id = id
        self.slug = slug
        self.publication_slug = publication_slug


class ReleaseFilesGenerator(object):

    def __init__(self, legacy_structure):
        self.legacy_structure = legacy_structure

        # Instantiate a new ContainerClient for the emulator
        self.blob_service_client = BlobServiceClient.from_connection_string(
            "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://data-storage:10000/devstoreaccount1")

        exclusions_release = Release(
            "e7774a74-1f62-4b76-b9b5-84f14dac7278", "2016-17", "permanent-and-fixed-period-exclusions-in-england")
        pupil_absence_release = Release(
            "4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5", "2016-17", "pupil-absence-in-schools-in-england")

        self.all_files_zip_files = [
            # Exclusions
            AncillaryFile(exclusions_release,
                          "permanent-and-fixed-period-exclusions-in-england_2016-17.zip",
                          "All files"),

            # Pupil absence
            AncillaryFile(pupil_absence_release,
                          "pupil-absence-in-schools-in-england_2016-17.zip",
                          "All files")
        ]

        self.data_files = [
            # Exclusions
            DataFile(exclusions_release,
                     "0ddf5b95-874b-4bcd-6a8f-08d7ec2d8168",
                     "Duration of fixed exclusions",
                     "exclusions_duration_of_fixed_exclusions.csv"),
            DataFile(exclusions_release,
                     "a5eb34a2-4615-4ee4-6a90-08d7ec2d8168",
                     "Exclusions by characteristic",
                     "exclusions_by_characteristic.csv"),
            DataFile(exclusions_release,
                     "0d8b6e60-7b92-4f4d-6a91-08d7ec2d8168",
                     "Exclusions by geographic level",
                     "exclusions_by_geographic_level.csv"),
            DataFile(exclusions_release,
                     "25b691c9-4d07-437f-6a93-08d7ec2d8168",
                     "Number of fixed exclusions",
                     "exclusions_number_of_fixed_exclusions.csv"),
            DataFile(exclusions_release,
                     "f5c56849-f634-4fda-6a92-08d7ec2d8168",
                     "Exclusions by reason",
                     "exclusions_by_reason.csv"),
            DataFile(exclusions_release,
                     "f0538c07-fc64-423b-6a94-08d7ec2d8168",
                     "Total days missed due to fixed period exclusions",
                     "exclusions_total_days_missed_fixed_exclusions.csv"),

            # Pupil absence
            DataFile(pupil_absence_release,
                     "15c05193-1b4a-4acb-6a81-08d7ec2d8168",
                     "Absence by characteristic",
                     "absence_by_characteristic.csv"),
            DataFile(pupil_absence_release,
                     "cf0e3cb5-ea9c-45b1-6a83-08d7ec2d8168",
                     "Absence by term",
                     "absence_by_term.csv"),
            DataFile(pupil_absence_release,
                     "d4d72886-8921-426a-6a82-08d7ec2d8168",
                     "Absence by geographic level",
                     "absence_by_geographic_level.csv"),
            DataFile(pupil_absence_release,
                     "bcf86eb2-4244-41aa-6a84-08d7ec2d8168",
                     "Absence for four year olds",
                     "absence_for_four_year_olds.csv"),
            DataFile(pupil_absence_release,
                     "c1239606-300a-4782-6a85-08d7ec2d8168",
                     "Absence in prus",
                     "absence_in_prus.csv"),
            DataFile(pupil_absence_release, "c8141534-d706-49d0-6a86-08d7ec2d8168",
                     "Absence number missing at least one session by reason",
                     "absence_number_missing_at_least_one_session_by_reason.csv"),
            DataFile(pupil_absence_release,
                     "28199189-f0b9-44f2-6a87-08d7ec2d8168",
                     "Absence rate percent bands",
                     "absence_rate_percent_bands.csv")
        ]

        self.metadata_files = [
            # Exclusions
            MetadataFile(exclusions_release,
                         "2a90141f-3331-4f00-6a98-08d7ec2d8168",
                         filename="exclusions_duration_of_fixed_exclusions.meta.csv"),
            MetadataFile(exclusions_release,
                         "04b45150-0492-4b8b-6a95-08d7ec2d8168",
                         filename="exclusions_by_characteristic.meta.csv"),
            MetadataFile(exclusions_release,
                         "7af9b369-1487-41f4-6a96-08d7ec2d8168",
                         filename="exclusions_by_geographic_level.meta.csv"),
            MetadataFile(exclusions_release,
                         "8a7d0775-8b51-47e3-6a99-08d7ec2d8168",
                         filename="exclusions_number_of_fixed_exclusions.meta.csv"),
            MetadataFile(exclusions_release,
                         "5a22f935-6d50-4d76-6a97-08d7ec2d8168",
                         filename="exclusions_by_reason.meta.csv"),
            MetadataFile(exclusions_release,
                         "9a917320-891c-4a00-6a9a-08d7ec2d8168",
                         filename="exclusions_total_days_missed_fixed_exclusions.meta.csv"),

            # Pupil absence
            MetadataFile(pupil_absence_release,
                         "be141b27-0143-41b0-6a88-08d7ec2d8168",
                         filename="absence_by_characteristic.meta.csv"),
            MetadataFile(pupil_absence_release,
                         "c2a67d23-b135-4d84-6a8a-08d7ec2d8168",
                         filename="absence_by_term.meta.csv"),
            MetadataFile(pupil_absence_release,
                         "5f355f7c-a6ec-481a-6a89-08d7ec2d8168",
                         filename="absence_by_geographic_level.meta.csv"),
            MetadataFile(pupil_absence_release,
                         "f06041a4-e468-44df-6a8b-08d7ec2d8168",
                         filename="absence_for_four_year_olds.meta.csv"),
            MetadataFile(pupil_absence_release,
                         "ec069435-bdc0-4c2d-6a8c-08d7ec2d8168",
                         filename="absence_in_prus.meta.csv"),
            MetadataFile(pupil_absence_release,
                         "f9c1eecc-e0a0-40cd-6a8d-08d7ec2d8168",
                         filename="absence_number_missing_at_least_one_session_by_reason.meta.csv"),
            MetadataFile(pupil_absence_release,
                         "36c7efeb-ffe9-4444-6a8e-08d7ec2d8168",
                         filename="absence_rate_percent_bands.meta.csv"),
        ]

    def create_public_release_files(self):
        try:
            container_client = self.blob_service_client.create_container(
                "downloads")
        except ResourceExistsError:
            container_client = self.blob_service_client.get_container_client(
                "downloads")

        self.upload_files(container_client,
                          self.all_files_zip_files,
                          public_storage=True)
        self.upload_files(container_client,
                          self.data_files,
                          public_storage=True)

    def create_private_release_files(self):
        try:
            container_client = self.blob_service_client.create_container(
                "releases")
        except ResourceExistsError:
            container_client = self.blob_service_client.get_container_client(
                "releases")

        self.upload_files(container_client,
                          self.data_files,
                          public_storage=False)
        self.upload_files(container_client,
                          self.metadata_files,
                          public_storage=False)

    def upload_files(self, container_client, files, public_storage):
        data = b'abcd'*128
        for file in files:
            path = file.path() if not self.legacy_structure else (
                file.legacy_public_path() if public_storage else file.legacy_private_path())
            blob_client = container_client.get_blob_client(path)
            blob_client.upload_blob(
                data, blob_type="BlockBlob", metadata=file.metadata())


if __name__ == '__main__':
    # EES-1704 Release file structure migration
    # In case database hasn't been manually migrated yet,
    # allow setting legacy_structure to true to create the old structure
    generator = ReleaseFilesGenerator(legacy_structure=False)
    generator.create_public_release_files()
    generator.create_private_release_files()
