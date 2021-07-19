import {
  ReleaseDataFileRouteParams,
  ReleaseRouteParams,
  releaseDataRoute,
} from '@admin/routes/releaseRoutes';
import WarningMessage from '@common/components/WarningMessage';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import useFormSubmit from '@common/hooks/useFormSubmit';
import React from 'react';
import releaseDataFileService from '@admin/services/releaseDataFileService';
import Link from '@admin/components/Link';
import { generatePath, RouteComponentProps } from 'react-router';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Yup from '@common/validation/yup';
import { Formik } from 'formik';
import { Form, FormFieldTextInput } from '@common/components/form';
import Button from '@common/components/Button';

interface FormValues {
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

  const handleSubmit = useFormSubmit<FormValues>(async values => {
    await releaseDataFileService.updateFile(releaseId, fileId, {
      title: values.title,
    });

    history.push(
      generatePath<ReleaseRouteParams>(releaseDataRoute.path, {
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
        to={generatePath<ReleaseRouteParams>(releaseDataRoute.path, {
          publicationId,
          releaseId,
        })}
      >
        Back
      </Link>

      <LoadingSpinner loading={dataFileLoading}>
        <section>
          <h2>Edit data file details</h2>

          {dataFile ? (
            <Formik<FormValues>
              initialValues={{ title: dataFile.title }}
              validationSchema={Yup.object<FormValues>({
                title: Yup.string().required('Enter a title'),
              })}
              onSubmit={handleSubmit}
            >
              <Form id="dataFileForm">
                <FormFieldTextInput<FormValues>
                  className="govuk-!-width-two-thirds"
                  label="Title"
                  name="title"
                />

                <Button type="submit">Save changes</Button>
              </Form>
            </Formik>
          ) : (
            <WarningMessage>Could not load data file details</WarningMessage>
          )}
        </section>
      </LoadingSpinner>
    </>
  );
};

export default ReleaseDataFilePage;
