import os

class File:
    def __init__(self, path, name):
        self.path = path
        self.name = name

class ReleaseFilesGenerator(object):

    def create_public_release_files(self):

        from azure.storage.blob import ContainerClient
        from datetime import datetime, timezone

        # Instantiate a new ContainerClient for the emulator
        container_client = ContainerClient.from_connection_string("DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://data-storage:10000/devstoreaccount1", "downloads")

        files = [
            # Exclusions
            # All files zip
            File("permanent-and-fixed-period-exclusions-in-england/2016-17/ancillary/permanent-and-fixed-period-exclusions-in-england_2016-17.zip", "All files"),
            # Data files
            File("permanent-and-fixed-period-exclusions-in-england/2016-17/data/exclusions_duration_of_fixed_exclusions.csv", "Duration of fixed exclusions"),
            File("permanent-and-fixed-period-exclusions-in-england/2016-17/data/exclusions_by_characteristic.csv", "Exclusions by characteristic"),
            File("permanent-and-fixed-period-exclusions-in-england/2016-17/data/exclusions_by_geographic_level.csv", "Exclusions by geographic level"),
            File("permanent-and-fixed-period-exclusions-in-england/2016-17/data/exclusions_number_of_fixed_exclusions.csv", "Number of fixed exclusions"),
            File("permanent-and-fixed-period-exclusions-in-england/2016-17/data/exclusions_by_reason.csv", "Exclusions by reason"),
            File("permanent-and-fixed-period-exclusions-in-england/2016-17/data/exclusions_total_days_missed_fixed_exclusions.csv", "Total days missed due to fixed period exclusions"),

            # Pupil absence
            # All files zip
            File("pupil-absence-in-schools-in-england/2016-17/ancillary/pupil-absence-in-schools-in-england_2016-17.zip", "All files"),
            # Data files
            File("pupil-absence-in-schools-in-england/2016-17/data/absence_by_characteristic.csv", "Absence by characteristic"),
            File("pupil-absence-in-schools-in-england/2016-17/data/absence_by_term.csv", "Absence by term"),
            File("pupil-absence-in-schools-in-england/2016-17/data/absence_by_geographic_level.csv", "Absence by geographic level"),
            File("pupil-absence-in-schools-in-england/2016-17/data/absence_for_four_year_olds.csv", "Absence for four year olds"),
            File("pupil-absence-in-schools-in-england/2016-17/data/absence_in_prus.csv", "Absence in prus"),
            File("pupil-absence-in-schools-in-england/2016-17/data/absence_number_missing_at_least_one_session_by_reason.csv", "Absence number missing at least one session by reason"),
            File("pupil-absence-in-schools-in-england/2016-17/data/absence_rate_percent_bands.csv", "Absence rate percent bands")]

        # Create a timestamp matching "yyyy-MM-ddTHH:mm:ss.mmmmmmmZ"
        releasedatetime = datetime.now(timezone.utc).isoformat().replace("+00:00", "0Z")
        data = b'abcd'*128
        for file in files:
            blob_client = container_client.get_blob_client(file.path)
            blob_client.upload_blob(data, blob_type="BlockBlob", metadata={
                "name": file.name,
                "releasedatetime": releasedatetime
                })

if __name__ == '__main__':
    generator = ReleaseFilesGenerator()
    generator.create_public_release_files()
