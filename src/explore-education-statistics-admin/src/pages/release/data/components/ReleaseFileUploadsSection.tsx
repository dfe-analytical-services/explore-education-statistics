import Link from '@admin/components/Link';
import AncillaryFileDetailsTable from '@admin/pages/release/data/components/AncillaryFileDetailsTable';
import AncillaryFileForm, {
  AncillaryFileFormValues,
} from '@admin/pages/release/data/components/AncillaryFileForm';
import releaseAncillaryFileQueries from '@admin/queries/releaseAncillaryFileQueries';
import {
  releaseAncillaryFileRoute,
  ReleaseAncillaryFileRouteParams,
} from '@admin/routes/releaseRoutes';
import releaseAncillaryFileService, {
  AncillaryFile,
} from '@admin/services/releaseAncillaryFileService';
import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import ButtonText from '@common/components/ButtonText';
import InsetText from '@common/components/InsetText';
import LoadingSpinner from '@common/components/LoadingSpinner';
import ModalConfirm from '@common/components/ModalConfirm';
import WarningMessage from '@common/components/WarningMessage';
import logger from '@common/services/logger';
import Yup from '@common/validation/yup';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import React, { useCallback, useMemo, useState } from 'react';
import { generatePath } from 'react-router';

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
          Ancillary files are additional files attached to the release for users
          to download. They will appear in the associated files list on the
          release page and the download files page.
        </p>
      </InsetText>

      {canUpdateRelease ? (
        <AncillaryFileForm
          files={files}
          submitText="Add file"
          resetAfterSubmit
          validationSchema={Yup.object({
            file: Yup.file().required('Choose a file'),
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

                  <AncillaryFileDetailsTable
                    ancillaryFile={file}
                    releaseId={releaseId}
                  >
                    {canUpdateRelease && (
                      <>
                        <Link
                          className="govuk-!-margin-right-4"
                          to={generatePath<ReleaseAncillaryFileRouteParams>(
                            releaseAncillaryFileRoute.path,
                            {
                              publicationId,
                              releaseId,
                              fileId: file.id,
                            },
                          )}
                        >
                          Edit file
                        </Link>
                        <ButtonText onClick={() => setDeleteFile(file)}>
                          Delete file
                        </ButtonText>
                      </>
                    )}
                  </AncillaryFileDetailsTable>
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
