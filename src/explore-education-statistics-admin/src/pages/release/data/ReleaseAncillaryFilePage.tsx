import {
  ReleaseAncillaryFileRouteParams,
  ReleaseRouteParams,
  releaseDataAncillaryRoute,
} from '@admin/routes/releaseRoutes';
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

interface EditFormValues {
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
        {ancillaryFile && (
          <section>
            <h2>Edit ancillary file details</h2>
            <Formik<EditFormValues>
              initialValues={{ title: ancillaryFile.title }}
              validationSchema={Yup.object<EditFormValues>({
                title: Yup.string().required('Enter a ancillary title'),
              })}
              onSubmit={async values => {
                await releaseAncillaryFileService.updateFile(
                  releaseId,
                  fileId,
                  { name: values.title },
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
              {form => (
                <Form {...form} id="edit-data-file-form">
                  <FormFieldTextInput id="title" label="Title" name="title" />
                  <Button type="submit">Save changes</Button>
                </Form>
              )}
            </Formik>
          </section>
        )}
      </LoadingSpinner>
    </>
  );
};

export default ReleaseAncillaryFilePage;
