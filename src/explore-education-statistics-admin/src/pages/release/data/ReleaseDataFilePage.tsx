import {
  ReleaseDataFileRouteParams,
  ReleaseRouteParams,
  releaseDataRoute,
} from '@admin/routes/releaseRoutes';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import React from 'react';
import releaseDataFileService from '@admin/services/releaseDataFileService';
import Link from '@admin/components/Link';
import { generatePath, RouteComponentProps } from 'react-router';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Yup from '@common/validation/yup';
import { Formik } from 'formik';
import { Form, FormFieldTextInput } from '@common/components/form';
import Button from '@common/components/Button';

interface EditSubjectFormValues {
  title: string;
}

const ReleaseDataFilePage = ({
  history,
  match: {
    params: { publicationId, releaseId, fileId },
  },
}: RouteComponentProps<ReleaseDataFileRouteParams>) => {
  const {
    value: dataFile,
    isLoading: dataFileLoading,
  } = useAsyncHandledRetry(
    () => releaseDataFileService.getDataFile(releaseId, fileId),
    [releaseId, fileId],
  );

  return (
    <>
      <Link
        className="govuk-!-margin-bottom-6"
        back
        to={generatePath<ReleaseRouteParams>(releaseDataRoute.path, {
          publicationId,
          releaseId,
        })}
      >
        Back
      </Link>
      <LoadingSpinner loading={dataFileLoading}>
        {dataFile && (
          <section>
            <h2>Edit data file details</h2>
            <Formik<EditSubjectFormValues>
              initialValues={{ title: dataFile.title }}
              validationSchema={Yup.object<EditSubjectFormValues>({
                title: Yup.string().required('Enter a subject title'),
              })}
              onSubmit={async values => {
                await releaseDataFileService.updateFile(releaseId, fileId, {
                  name: values.title,
                });

                history.push(
                  generatePath<ReleaseRouteParams>(releaseDataRoute.path, {
                    publicationId,
                    releaseId,
                  }),
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

export default ReleaseDataFilePage;
