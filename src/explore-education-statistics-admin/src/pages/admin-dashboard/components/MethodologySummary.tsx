import publicationService, {
  ExternalMethodology,
  MyPublication,
} from '@admin/services/publicationService';
import ButtonGroup from '@common/components/ButtonGroup';
import Button from '@common/components/Button';
import ButtonLink from '@admin/components/ButtonLink';
import Link from '@admin/components/Link';
import { methodologyCreateRoute } from '@admin/routes/routes';
import { methodologySummaryRoute } from '@admin/routes/methodologyRoutes';
import React, { useState } from 'react';
import { generatePath } from 'react-router';
import MethodologyExternalLinkForm from './MethodologyExternalLinkForm';

export interface Props {
  publication: MyPublication;
  topicId: string;
  onChangePublication: () => void;
}

const MethodologySummary = ({
  publication,
  topicId,
  onChangePublication,
}: Props) => {
  const [
    showAddExternalMethodologyForm,
    setShowAddExternalMethodologyForm,
  ] = useState<boolean>();
  const [
    showEditExternalMethodologyForm,
    setShowEditExternalMethodologyForm,
  ] = useState<boolean>();

  const { contact, externalMethodology, methodology, id, title } = publication;

  const handleExternalMethodologySubmit = async (
    values: ExternalMethodology,
  ) => {
    const updatedPublication = {
      title,
      contact: {
        contactName: contact?.contactName ?? '',
        contactTelNo: contact?.contactTelNo ?? '',
        teamEmail: contact?.teamEmail ?? '',
        teamName: contact?.teamName ?? '',
      },
      topicId,
      externalMethodology: {
        title: values.title,
        url: values.url,
      },
    };

    await publicationService.updatePublication(id, updatedPublication);
    onChangePublication();
  };

  const handleRemoveExternalMethodology = async () => {
    const updatedPublication = {
      title,
      contact: {
        contactName: contact?.contactName ?? '',
        contactTelNo: contact?.contactTelNo ?? '',
        teamEmail: contact?.teamEmail ?? '',
        teamName: contact?.teamName ?? '',
      },
      topicId,
    };

    await publicationService.updatePublication(id, updatedPublication);
    onChangePublication();
  };

  return (
    <>
      {methodology ? (
        <Link
          to={generatePath(methodologySummaryRoute.path, {
            publicationId: id,
            methodologyId: methodology.id,
          })}
        >
          {methodology.title}
        </Link>
      ) : (
        <>
          {externalMethodology?.url ? (
            <>
              {!showEditExternalMethodologyForm && (
                <>
                  <Link to={externalMethodology.url} unvisited>
                    {externalMethodology.title} (external methodology)
                  </Link>
                  <ButtonGroup className="govuk-!-margin-bottom-2 govuk-!-margin-top-2">
                    <Button
                      type="button"
                      onClick={() => setShowEditExternalMethodologyForm(true)}
                    >
                      Edit
                    </Button>
                    <Button
                      type="button"
                      variant="warning"
                      onClick={handleRemoveExternalMethodology}
                    >
                      Remove
                    </Button>
                  </ButtonGroup>
                </>
              )}
              {showEditExternalMethodologyForm && (
                <MethodologyExternalLinkForm
                  initialValues={externalMethodology}
                  onCancel={() => setShowEditExternalMethodologyForm(false)}
                  onSubmit={values => {
                    handleExternalMethodologySubmit(values);
                    setShowEditExternalMethodologyForm(false);
                  }}
                />
              )}
            </>
          ) : (
            <>
              {!showAddExternalMethodologyForm && (
                <ButtonGroup className="govuk-!-margin-bottom-2">
                  <ButtonLink
                    to={generatePath(methodologyCreateRoute.path, {
                      publicationId: id,
                    })}
                    data-testid={`Create methodology for ${title}`}
                  >
                    Create methodology
                  </ButtonLink>
                  <Button
                    type="button"
                    data-testid={`Link methodology for ${title}`}
                    onClick={() => setShowAddExternalMethodologyForm(true)}
                  >
                    Link to an externally hosted methodology
                  </Button>
                </ButtonGroup>
              )}
              {showAddExternalMethodologyForm && (
                <MethodologyExternalLinkForm
                  onCancel={() => setShowAddExternalMethodologyForm(false)}
                  onSubmit={values => {
                    handleExternalMethodologySubmit(values);
                    setShowAddExternalMethodologyForm(false);
                  }}
                />
              )}
            </>
          )}
        </>
      )}
    </>
  );
};

export default MethodologySummary;
