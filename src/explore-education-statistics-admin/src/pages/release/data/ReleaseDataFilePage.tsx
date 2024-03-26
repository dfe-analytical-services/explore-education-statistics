import {
  ReleaseDataFileRouteParams,
  ReleaseRouteParams,
  releaseDataRoute,
} from '@admin/routes/releaseRoutes';
import RHFFormFieldTextInput from '@common/components/form/rhf/RHFFormFieldTextInput';
import FormProvider from '@common/components/form/rhf/FormProvider';
import RHFForm from '@common/components/form/rhf/RHFForm';
import WarningMessage from '@common/components/WarningMessage';
import useAsyncRetry from '@common/hooks/useAsyncRetry';
import React from 'react';
import releaseDataFileService from '@admin/services/releaseDataFileService';
import Link from '@admin/components/Link';
import { generatePath, RouteComponentProps } from 'react-router';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Yup from '@common/validation/yup';
import Button from '@common/components/Button';

interface FormValues {
  title: string;
}

export default function ReleaseDataFilePage({
  history,
  match: {
    params: { publicationId, releaseId, fileId },
  },
}: RouteComponentProps<ReleaseDataFileRouteParams>) {
  const { value: dataFile, isLoading: dataFileLoading } = useAsyncRetry(
    () => releaseDataFileService.getDataFile(releaseId, fileId),
    [releaseId, fileId],
  );

  const handleSubmit = async (values: FormValues) => {
    await releaseDataFileService.updateFile(releaseId, fileId, {
      title: values.title,
    });

    history.push(
      generatePath<ReleaseRouteParams>(releaseDataRoute.path, {
        publicationId,
        releaseId,
      }),
    );
  };

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
            <FormProvider
              initialValues={{ title: dataFile.title }}
              validationSchema={Yup.object<FormValues>({
                title: Yup.string().required('Enter a title'),
              })}
            >
              <RHFForm id="dataFileForm" onSubmit={handleSubmit}>
                <RHFFormFieldTextInput<FormValues>
                  className="govuk-!-width-two-thirds"
                  label="Title"
                  name="title"
                />

                <Button type="submit">Save changes</Button>
              </RHFForm>
            </FormProvider>
          ) : (
            <WarningMessage>Could not load data file details</WarningMessage>
          )}
        </section>
      </LoadingSpinner>
    </>
  );
}
