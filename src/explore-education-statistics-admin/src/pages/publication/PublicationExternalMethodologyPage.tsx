import ExternalMethodologyForm from '@admin/pages/methodology/external-methodology/components/ExternalMethodologyForm';
import usePublicationContext from '@admin/pages/publication/contexts/PublicationContext';
import {
  PublicationRouteParams,
  publicationMethodologiesRoute,
} from '@admin/routes/publicationRoutes';
import publicationService, {
  ExternalMethodology,
  UpdatePublicationRequest,
} from '@admin/services/publicationService';
import React from 'react';
import { generatePath, useHistory } from 'react-router';

const PublicationExternalMethodologyPage = () => {
  const history = useHistory();
  const { publicationId, publication, onReload } = usePublicationContext();
  const { externalMethodology } = publication;

  const returnRoute = generatePath<PublicationRouteParams>(
    publicationMethodologiesRoute.path,
    {
      publicationId,
    },
  );

  const handleExternalMethodologySubmit = async (
    values: ExternalMethodology,
  ) => {
    if (!publication) {
      return;
    }
    const updatedPublication: UpdatePublicationRequest = {
      title: publication.title,
      contact: {
        contactName: publication.contact?.contactName ?? '',
        contactTelNo: publication.contact?.contactTelNo ?? '',
        teamEmail: publication.contact?.teamEmail ?? '',
        teamName: publication.contact?.teamName ?? '',
      },
      topicId: publication.topicId,
      externalMethodology: {
        title: values.title,
        url: values.url,
      },
    };

    await publicationService.updatePublication(
      publicationId,
      updatedPublication,
    );
    onReload();
    history.push(returnRoute);
  };

  return (
    <>
      <h2>
        {externalMethodology
          ? 'Edit external methodology link'
          : 'Link to an externally hosted methodology'}
      </h2>
      <ExternalMethodologyForm
        initialValues={externalMethodology}
        onCancel={() => history.push(returnRoute)}
        onSubmit={handleExternalMethodologySubmit}
      />
    </>
  );
};

export default PublicationExternalMethodologyPage;
