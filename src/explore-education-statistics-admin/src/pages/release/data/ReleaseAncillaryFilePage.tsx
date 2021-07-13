import {
  ReleaseAncillaryFileRouteParams,
  ReleaseRouteParams,
  releaseDataAncillaryRoute,
} from '@admin/routes/releaseRoutes';
import WarningMessage from '@common/components/WarningMessage';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import React from 'react';
import Link from '@admin/components/Link';
import { generatePath, RouteComponentProps } from 'react-router';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Yup from '@common/validation/yup';
import { Formik } from 'formik';
import { Form, FormFieldTextInput } from '@common/components/form';
import Button from '@common/components/Button';
import releaseAncillaryFileService from '@admin/services/releaseAncillaryFileService';

interface FormValues {
  title: string;
}

const ReleaseAncillaryFilePage = ({
  history,
  match: {
    params: { publicationId, releaseId, fileId },
  },
}: RouteComponentProps<ReleaseAncillaryFileRouteParams>) => {
  const {
    value: ancillaryFile,
    isLoading: ancillaryFileLoading,
  } = useAsyncHandledRetry(
    () => releaseAncillaryFileService.getAncillaryFile(releaseId, fileId),
    [releaseId, fileId],
  );

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
              initialValues={{ title: ancillaryFile.title }}
              validationSchema={Yup.object<FormValues>({
                title: Yup.string().required('Enter a title'),
              })}
              onSubmit={async values => {
                await releaseAncillaryFileService.updateFile(
                  releaseId,
                  fileId,
                  { title: values.title },
                );

                history.push(
                  generatePath<ReleaseRouteParams>(
                    releaseDataAncillaryRoute.path,
                    {
                      publicationId,
                      releaseId,
                    },
                  ),
                );
              }}
            >
              <Form id="ancillaryFileForm">
                <FormFieldTextInput<FormValues>
                  className="govuk-!-width-one-half"
                  label="Title"
                  name="title"
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
