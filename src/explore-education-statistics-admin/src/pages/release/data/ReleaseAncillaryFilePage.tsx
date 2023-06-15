import {
  ReleaseAncillaryFileRouteParams,
  ReleaseRouteParams,
  releaseDataAncillaryRoute,
} from '@admin/routes/releaseRoutes';
import WarningMessage from '@common/components/WarningMessage';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import useFormSubmit from '@common/hooks/useFormSubmit';
import Link from '@admin/components/Link';
import { generatePath, useHistory, useParams } from 'react-router';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Yup from '@common/validation/yup';
import { Formik } from 'formik';
import {
  Form,
  FormFieldTextArea,
  FormFieldTextInput,
} from '@common/components/form';
import Button from '@common/components/Button';
import releaseAncillaryFileService from '@admin/services/releaseAncillaryFileService';
import React from 'react';

interface FormValues {
  title: string;
  summary: string;
}

const ReleaseAncillaryFilePage = () => {
  const history = useHistory();
  const { publicationId, releaseId, fileId } =
    useParams<ReleaseAncillaryFileRouteParams>();

  const { value: ancillaryFile, isLoading: ancillaryFileLoading } =
    useAsyncHandledRetry(
      () => releaseAncillaryFileService.getAncillaryFile(releaseId, fileId),
      [releaseId, fileId],
    );

  const handleSubmit = useFormSubmit<FormValues>(async ({ title, summary }) => {
    await releaseAncillaryFileService.updateFile(releaseId, fileId, {
      title,
      summary,
    });

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
          <h2>Edit ancillary file details</h2>

          {ancillaryFile ? (
            <Formik<FormValues>
              initialValues={{
                title: ancillaryFile.title,
                summary: ancillaryFile.summary,
              }}
              validationSchema={Yup.object<FormValues>({
                title: Yup.string().required('Enter a title'),
                summary: Yup.string().required('Enter a summary'),
              })}
              onSubmit={handleSubmit}
            >
              <Form id="ancillaryFileForm">
                <FormFieldTextInput<FormValues>
                  className="govuk-!-width-one-half"
                  label="Title"
                  name="title"
                />

                <FormFieldTextArea<FormValues>
                  className="govuk-!-width-one-half"
                  label="Summary"
                  name="summary"
                />

                <Button type="submit">Save changes</Button>
              </Form>
            </Formik>
          ) : (
            <WarningMessage>
              Could not load ancillary file details
            </WarningMessage>
          )}
        </section>
      </LoadingSpinner>
    </>
  );
};

export default ReleaseAncillaryFilePage;
