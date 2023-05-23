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
import { generatePath, RouteComponentProps } from 'react-router';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Yup from '@common/validation/yup';
import { Formik } from 'formik';
import { Form } from '@common/components/form';
import Button from '@common/components/Button';
import releaseAncillaryFileService from '@admin/services/releaseAncillaryFileService';
import FormFieldFileInput from '@common/components/form/FormFieldFileInput';
import ButtonText from '@common/components/ButtonText';
import FormattedDate from '@common/components/FormattedDate';

interface FormValues {
  file: File | null;
}

const ReleaseAncillaryFileReplacePage = ({
  history,
  match: {
    params: { publicationId, releaseId, fileId },
  },
}: RouteComponentProps<ReleaseAncillaryFileReplaceRouteParams>) => {
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

              <table>
                <tbody>
                  <tr>
                    <th scope="row" className="gov-!-width-one-third">
                      Title
                    </th>
                    <td>{ancillaryFile.title}</td>
                  </tr>
                  <tr>
                    <th scope="row" className="gov-!-width-one-third">
                      File
                    </th>
                    <td>
                      <ButtonText
                        onClick={() =>
                          releaseAncillaryFileService.downloadFile(
                            releaseId,
                            ancillaryFile.id,
                            ancillaryFile.filename,
                          )
                        }
                      >
                        {ancillaryFile.filename}
                      </ButtonText>
                    </td>
                  </tr>
                  <tr>
                    <th scope="row" className="gov-!-width-one-third">
                      File size
                    </th>
                    <td>{`${ancillaryFile.fileSize.size.toLocaleString()} ${
                      ancillaryFile.fileSize.unit
                    }`}</td>
                  </tr>
                  <tr>
                    <th scope="row" className="gov-!-width-one-third">
                      Uploaded by
                    </th>
                    <td>
                      <a href={`mailto:${ancillaryFile.userName}`}>
                        {ancillaryFile.userName}
                      </a>
                    </td>
                  </tr>
                  <tr>
                    <th scope="row" className="gov-!-width-one-third">
                      Date uploaded
                    </th>
                    <td>
                      <FormattedDate format="d MMMM yyyy HH:mm">
                        {ancillaryFile.created}
                      </FormattedDate>
                    </td>
                  </tr>
                  <tr>
                    <th scope="row" className="gov-!-width-one-third">
                      Summary
                    </th>
                    <td>
                      <div className="dfe-white-space--pre-wrap">
                        {ancillaryFile.summary}
                      </div>
                    </td>
                  </tr>
                </tbody>
              </table>

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
