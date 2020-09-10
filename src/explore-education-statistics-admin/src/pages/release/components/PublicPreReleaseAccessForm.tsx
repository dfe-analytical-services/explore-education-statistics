import FormFieldEditor from '@admin/components/form/FormFieldEditor';
import { useConfig } from '@admin/contexts/ConfigContext';
import useFormSubmit from '@admin/hooks/useFormSubmit';
import styles from '@admin/pages/release/components/PublicPreReleaseAccessForm.module.scss';
import { ReleaseRouteParams } from '@admin/routes/releaseRoutes';
import { preReleaseRoute } from '@admin/routes/routes';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import { Form } from '@common/components/form';
import SanitizeHtml from '@common/components/SanitizeHtml';
import UrlContainer from '@common/components/UrlContainer';
import WarningMessage from '@common/components/WarningMessage';
import useToggle from '@common/hooks/useToggle';
import { Formik } from 'formik';
import React from 'react';
import { generatePath } from 'react-router';

const defaultAccessListText = `
<p>Beside Department for Education (DfE) professional and production staff the 
following post holders are given pre-release access up to 24 hours before release.</p>

<ul>
    <li>ADD ROLES HERE</li>
</ul>
  `;

const formId = 'publicPreReleaseAccessForm';

interface FormValues {
  preReleaseAccessList: string;
}

interface Props {
  publicationId: string;
  publicationSlug: string;
  releaseId: string;
  releaseSlug: string;
  isReleaseLive?: boolean;
  preReleaseAccessList: string;
  onSubmit: (values: FormValues) => void;
}

const PublicPreReleaseAccessForm = ({
  publicationId,
  publicationSlug,
  releaseId,
  releaseSlug,
  isReleaseLive = false,
  preReleaseAccessList,
  onSubmit,
}: Props) => {
  const [showForm, toggleForm] = useToggle(false);
  const { PublicAppUrl } = useConfig();

  const handleSubmit = useFormSubmit<FormValues>(async values => {
    await onSubmit(values);
    toggleForm.off();
  });

  return (
    <>
      {!isReleaseLive && (
        <div className="govuk-inset-text">
          <h3 className="govuk-heading-m">Before you start</h3>

          <ul>
            <li>
              you can add a list of roles who have been granted pre-release
              access to this release
            </li>
            <li>
              this list will become publicly facing when the publication is
              released
            </li>
          </ul>
        </div>
      )}

      {showForm ? (
        <Formik<FormValues>
          initialValues={{
            preReleaseAccessList: preReleaseAccessList || defaultAccessListText,
          }}
          onSubmit={handleSubmit}
        >
          <Form id={formId}>
            <FormFieldEditor<FormValues>
              name="preReleaseAccessList"
              id={`${formId}-preReleaseAccessList`}
              label="Public access list"
            />

            <ButtonGroup>
              <Button type="submit">Save access list</Button>
              <Button variant="secondary" onClick={toggleForm}>
                Cancel
              </Button>
            </ButtonGroup>
          </Form>
        </Formik>
      ) : (
        <>
          {isReleaseLive && (
            <WarningMessage>
              This release has been published and can no longer be updated.
            </WarningMessage>
          )}

          {preReleaseAccessList && (
            <>
              <h3 className="govuk-heading-m">
                Public pre-release access list preview
              </h3>

              <SanitizeHtml
                className={styles.preview}
                dirtyHtml={preReleaseAccessList}
                testId="publicPreReleaseAccessListPreview"
              />
            </>
          )}

          {!isReleaseLive && (
            <Button onClick={toggleForm}>
              {`${
                preReleaseAccessList ? 'Edit' : 'Create'
              } public pre-release access list`}
            </Button>
          )}
        </>
      )}

      <hr />

      <h3>How to access the release</h3>

      {!isReleaseLive && (
        <>
          <p>
            The <strong>pre-release</strong> will be accessible at:
          </p>

          <p>
            <UrlContainer
              url={`${window.location.origin}${generatePath<ReleaseRouteParams>(
                preReleaseRoute.path,
                {
                  publicationId,
                  releaseId,
                },
              )}`}
            />
          </p>
        </>
      )}

      <p>
        The <strong>public release</strong> will be accessible at:
      </p>

      <p>
        <UrlContainer
          url={`${PublicAppUrl}/find-statistics/${publicationSlug}/${releaseSlug}`}
        />
      </p>
    </>
  );
};

export default PublicPreReleaseAccessForm;
