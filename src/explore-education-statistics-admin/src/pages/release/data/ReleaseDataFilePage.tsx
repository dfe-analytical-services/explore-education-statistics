import {
  ReleaseDataFileRouteParams,
  ReleaseRouteParams,
  releaseDataRoute,
} from '@admin/routes/releaseRoutes';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import FormProvider from '@common/components/form/FormProvider';
import Form from '@common/components/form/Form';
import WarningMessage from '@common/components/WarningMessage';
import useAsyncRetry from '@common/hooks/useAsyncRetry';
import React from 'react';
import releaseDataFileService from '@admin/services/releaseDataFileService';
import Link from '@admin/components/Link';
import { generatePath, RouteComponentProps } from 'react-router';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Yup from '@common/validation/yup';
import Button from '@common/components/Button';
import { useQueryClient } from '@tanstack/react-query';
import releaseDataFileQueries from '@admin/queries/releaseDataFileQueries';

const titleMaxLength = 120;

interface FormValues {
  title: string;
}

export default function ReleaseDataFilePage({
  history,
  match: {
    params: { publicationId, releaseVersionId, fileId },
  },
}: RouteComponentProps<ReleaseDataFileRouteParams>) {
  const { value: dataFile, isLoading: dataFileLoading } = useAsyncRetry(
    () => releaseDataFileService.getDataFile(releaseVersionId, fileId),
    [releaseVersionId, fileId],
  );

  const queryClient = useQueryClient();

  const handleSubmit = async (values: FormValues) => {
    await releaseDataFileService.updateFile(releaseVersionId, fileId, {
      title: values.title,
    });

    queryClient.removeQueries({
      queryKey: releaseDataFileQueries.list(releaseVersionId).queryKey,
    });
    queryClient.removeQueries({
      queryKey: releaseDataFileQueries.listUploads(releaseVersionId).queryKey,
    });
    await queryClient.invalidateQueries({
      queryKey: releaseDataFileQueries.list._def,
    });
    await queryClient.invalidateQueries({
      queryKey: releaseDataFileQueries.listUploads._def,
    });

    history.push(
      generatePath<ReleaseRouteParams>(releaseDataRoute.path, {
        publicationId,
        releaseVersionId,
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
          releaseVersionId,
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
                title: Yup.string()
                  .required('Enter a title')
                  .max(
                    titleMaxLength,
                    `Title must be ${titleMaxLength} characters or fewer`,
                  ),
              })}
            >
              <Form id="dataFileForm" onSubmit={handleSubmit}>
                <FormFieldTextInput<FormValues>
                  className="govuk-!-width-two-thirds"
                  label="Title"
                  name="title"
                  maxLength={titleMaxLength}
                />

                <Button type="submit">Save changes</Button>
              </Form>
            </FormProvider>
          ) : (
            <WarningMessage>Could not load data file details</WarningMessage>
          )}
        </section>
      </LoadingSpinner>
    </>
  );
}
