import AncillaryFileForm, {
  AncillaryFileFormValues,
} from '@admin/pages/release/data/components/AncillaryFileForm';
import AncillaryFileSummaryList from '@admin/pages/release/data/components/AncillaryFileSummaryList';
import releaseAncillaryFileQueries from '@admin/queries/releaseAncillaryFileQueries';
import releaseAncillaryFileService, {
  AncillaryFile,
} from '@admin/services/releaseAncillaryFileService';
import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import InsetText from '@common/components/InsetText';
import LoadingSpinner from '@common/components/LoadingSpinner';
import ModalConfirm from '@common/components/ModalConfirm';
import WarningMessage from '@common/components/WarningMessage';
import logger from '@common/services/logger';
import Yup from '@common/validation/yup';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import React, { useCallback, useMemo, useState } from 'react';

interface Props {
  publicationId: string;
  releaseId: string;
  canUpdateRelease: boolean;
}

export default function ReleaseFileUploadsSection({
  publicationId,
  releaseId,
  canUpdateRelease,
}: Props) {
  const [deleteFile, setDeleteFile] = useState<AncillaryFile>();

  const queryClient = useQueryClient();

  const listFilesQuery = useMemo(
    () => releaseAncillaryFileQueries.list(releaseId),
    [releaseId],
  );

  const { data: files = [], isLoading } = useQuery(listFilesQuery);

  const setFileDeleting = (fileToDelete: AncillaryFile, deleting: boolean) => {
    queryClient.setQueryData(
      listFilesQuery.queryKey,
      files.map(file =>
        file.filename !== fileToDelete.filename
          ? file
          : {
              ...file,
              isDeleting: deleting,
            },
      ),
    );
  };

  const handleSubmit = useCallback(
    async (values: AncillaryFileFormValues) => {
      const newFile = await releaseAncillaryFileService.createFile(releaseId, {
        title: values.title.trim(),
        file: values.file as File,
        summary: values.summary.trim(),
      });

      queryClient.setQueryData(listFilesQuery.queryKey, [...files, newFile]);
      await queryClient.invalidateQueries(listFilesQuery.queryKey);
    },
    [files, listFilesQuery.queryKey, queryClient, releaseId],
  );

  return (
    <>
      <h2>Add file to release</h2>
      <InsetText>
        <h3>Before you start</h3>
        <p>
          Supporting files are additional files attached to the release page for
          users to download. These should be used sparingly and only when there
          is no alternative. Please contact{' '}
          <a href="mailto:explore.statistics@education.gov.uk">
            explore.statistics@education.gov.uk
          </a>{' '}
          for advice if you are unsure.
        </p>
        <p>
          As the publisher you are responsible for the accessibility of any
          supporting files and ensuring that they are in line with the{' '}
          <a
            href="https://www.legislation.gov.uk/uksi/2018/852/contents/made"
            target="_blank"
            rel="noopener noreferrer"
          >
            Public Sector Bodies accessibility regulations 2018
          </a>
          .
        </p>
        <p>
          If you are attaching a spreadsheet not in CSV format, then you must
          review it against the{' '}
          <a
            href="https://analysisfunction.civilservice.gov.uk/policy-store/making-spreadsheets-accessible-a-brief-checklist-of-the-basics/"
            target="_blank"
            rel="noopener noreferrer"
          >
            Analytical Function checklist for accessible spreadsheets
          </a>
          .
        </p>
      </InsetText>

      {canUpdateRelease ? (
        <AncillaryFileForm
          files={files}
          submitText="Add file"
          resetAfterSubmit
          validationSchema={Yup.object<Partial<AncillaryFileFormValues>>({
            file: Yup.file()
              .required('Choose a file')
              .minSize(0, 'Choose a file that is not empty'),
          })}
          onSubmit={handleSubmit}
        />
      ) : (
        <WarningMessage>
          This release has been approved, and can no longer be updated.
        </WarningMessage>
      )}

      <hr className="govuk-!-margin-top-6 govuk-!-margin-bottom-6" />

      <h2>Uploaded files</h2>

      <LoadingSpinner loading={isLoading}>
        {files.length > 0 ? (
          <Accordion id="uploadedFiles">
            {files.map(file => (
              <AccordionSection
                key={file.title}
                heading={file.title}
                headingTag="h3"
              >
                <div style={{ position: 'relative' }}>
                  {file.isDeleting && (
                    <LoadingSpinner text="Deleting file" overlay />
                  )}

                  <AncillaryFileSummaryList
                    canUpdateRelease={canUpdateRelease}
                    file={file}
                    publicationId={publicationId}
                    releaseId={releaseId}
                    onDelete={() => setDeleteFile(file)}
                  />
                </div>
              </AccordionSection>
            ))}
          </Accordion>
        ) : (
          <InsetText>No files have been uploaded.</InsetText>
        )}
      </LoadingSpinner>

      {deleteFile && (
        <ModalConfirm
          open
          title="Confirm deletion of file"
          onExit={() => setDeleteFile(undefined)}
          onCancel={() => setDeleteFile(undefined)}
          onConfirm={async () => {
            setFileDeleting(deleteFile, true);
            setDeleteFile(undefined);

            try {
              await releaseAncillaryFileService.deleteFile(
                releaseId,
                deleteFile.id,
              );

              queryClient.setQueryData(
                listFilesQuery.queryKey,
                files.filter(file => file !== deleteFile),
              );

              await queryClient.invalidateQueries(listFilesQuery.queryKey);
            } catch (err) {
              logger.error(err);
              setFileDeleting(deleteFile, false);
            }
          }}
        >
          <p>
            This file will no longer be available for use in this release (
            {deleteFile.filename})
          </p>
        </ModalConfirm>
      )}
    </>
  );
}
