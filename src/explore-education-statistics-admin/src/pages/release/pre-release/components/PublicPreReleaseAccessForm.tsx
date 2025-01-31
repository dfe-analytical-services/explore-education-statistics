import FormFieldEditor from '@admin/components/form/FormFieldEditor';
import PreviewHtml from '@admin/components/PreviewHtml';
import styles from '@admin/pages/release/pre-release/components/PublicPreReleaseAccessForm.module.scss';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import FormProvider from '@common/components/form/FormProvider';
import Form from '@common/components/form/Form';
import InsetText from '@common/components/InsetText';
import WarningMessage from '@common/components/WarningMessage';
import useToggle from '@common/hooks/useToggle';
import React from 'react';

const defaultAccessListText = `
<p>Besides Department for Education (DfE) professional and production staff, the 
following post holders were given pre-release access up to 24 hours before release.</p>

<ul>
    <li>ADD ROLES HERE</li>
</ul>
  `;

const formId = 'publicPreReleaseAccessForm';

interface FormValues {
  preReleaseAccessList: string;
}

interface Props {
  canUpdateRelease?: boolean;
  isReleaseLive?: boolean;
  preReleaseAccessList: string;
  onSubmit: (values: FormValues) => void;
}

export default function PublicPreReleaseAccessForm({
  canUpdateRelease = false,
  isReleaseLive = false,
  preReleaseAccessList,
  onSubmit,
}: Props) {
  const [showForm, toggleForm] = useToggle(false);

  const handleSubmit = async (values: FormValues) => {
    await onSubmit(values);
    toggleForm.off();
  };

  return (
    <>
      {!isReleaseLive && (
        <InsetText>
          <h3 className="govuk-heading-m">Before you start</h3>
          <ul>
            <li>
              you should add a list of roles who have been granted pre-release
              access to this release
            </li>
            <li>
              this list will become publicly facing when the publication is
              released
            </li>
          </ul>
        </InsetText>
      )}

      {showForm ? (
        <FormProvider
          initialValues={{
            preReleaseAccessList: preReleaseAccessList || defaultAccessListText,
          }}
        >
          <Form id={formId} showErrorSummary={false} onSubmit={handleSubmit}>
            <FormFieldEditor<FormValues>
              name="preReleaseAccessList"
              label="Public access list"
              focusOnInit
              toolbarConfig={['bulletedList']}
            />

            <ButtonGroup>
              <Button type="submit">Save access list</Button>
              <Button variant="secondary" onClick={toggleForm}>
                Cancel
              </Button>
            </ButtonGroup>
          </Form>
        </FormProvider>
      ) : (
        <>
          {!canUpdateRelease && (
            <WarningMessage>
              This release has been approved, and can no longer be updated.
            </WarningMessage>
          )}

          {preReleaseAccessList && (
            <>
              <h3 className="govuk-heading-m">
                Public pre-release access list preview
              </h3>

              <PreviewHtml
                className={styles.preview}
                html={preReleaseAccessList}
                testId="publicPreReleaseAccessListPreview"
              />
            </>
          )}

          {canUpdateRelease && (
            <Button onClick={toggleForm} testId="access-list-btn">
              {`${
                preReleaseAccessList ? 'Edit' : 'Create'
              } public pre-release access list`}
            </Button>
          )}
        </>
      )}
    </>
  );
}
