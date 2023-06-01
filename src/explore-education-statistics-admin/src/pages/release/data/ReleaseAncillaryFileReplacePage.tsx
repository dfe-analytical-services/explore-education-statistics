import {
  ReleaseRouteParams,
  releaseDataAncillaryRoute,
  ReleaseAncillaryFileReplaceRouteParams,
} from '@admin/routes/releaseRoutes';
import WarningMessage from '@common/components/WarningMessage';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import useFormSubmit from '@common/hooks/useFormSubmit';
import React from 'react';
import Link from '@admin/components/Link';
import { generatePath } from 'react-router';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Yup from '@common/validation/yup';
import { Formik } from 'formik';
import { Form } from '@common/components/form';
import Button from '@common/components/Button';
import releaseAncillaryFileService from '@admin/services/releaseAncillaryFileService';
import FormFieldFileInput from '@common/components/form/FormFieldFileInput';
import AncillaryFileDetailsTable from '@admin/pages/release/data/components/AncillaryFileDetailsTable';
import { useHistory, useParams } from 'react-router-dom';

interface FormValues {
  file: File | null;
}

const ReleaseAncillaryFileReplacePage = () => {
  const history = useHistory();

  const { publicationId, releaseId, fileId } = useParams<
    ReleaseAncillaryFileReplaceRouteParams
  >();

  const {
    value: ancillaryFile,
    isLoading: ancillaryFileLoading,
  } = useAsyncHandledRetry(
    () => releaseAncillaryFileService.getAncillaryFile(releaseId, fileId),
    [releaseId, fileId],
  );

  const handleSubmit = useFormSubmit<FormValues>(async ({ file: newFile }) => {
    await releaseAncillaryFileService.replaceAncillaryFile(
      releaseId,
      fileId,
      newFile as File,
    );

    history.push(
      generatePath<ReleaseRouteParams>(releaseDataAncillaryRoute.path, {
        publicationId,
        releaseId,
      }),
    );
  });

  return (
    <>
      <Link
        className="govuk-!-margin-bottom-6"
        back
        to={generatePath<ReleaseRouteParams>(releaseDataAncillaryRoute.path, {
          publicationId,
          releaseId,
        })}
      >
        Back
      </Link>

      <LoadingSpinner loading={ancillaryFileLoading}>
        <section>
          <h2>Replace ancillary file</h2>

          {ancillaryFile ? (
            <>
              <h3>Ancillary file details</h3>

              <AncillaryFileDetailsTable
                ancillaryFile={ancillaryFile}
                releaseId={releaseId}
              />

              <h3>Upload replacement file</h3>

              <Formik<FormValues>
                initialValues={{
                  file: null,
                }}
                validationSchema={Yup.object<FormValues>({
                  file: Yup.file()
                    .required('Choose a file')
                    .minSize(0, 'Choose a file that is not empty'),
                })}
                onSubmit={handleSubmit}
              >
                {form => (
                  <Form id="ancillaryFileReplaceForm">
                    <FormFieldFileInput<FormValues>
                      name="file"
                      label="Upload file"
                    />

                    <Button type="submit" disabled={form.isSubmitting}>
                      Upload file
                    </Button>
                  </Form>
                )}
              </Formik>
            </>
          ) : (
            <WarningMessage>Could not load ancillary file</WarningMessage>
          )}
        </section>
      </LoadingSpinner>
    </>
  );
};

export default ReleaseAncillaryFileReplacePage;
