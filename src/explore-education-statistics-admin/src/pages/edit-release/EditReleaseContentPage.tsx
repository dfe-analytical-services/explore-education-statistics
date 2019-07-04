import React, { useEffect, useState } from 'react';
import { RouteComponentProps } from 'react-router';
import DummyPublicationsData from '@admin/pages/DummyPublicationsData';
import EditReleasePageTemplate from '@admin/pages/edit-release/components/EditReleasePageTemplate';
import {
  buildTablesRoute,
  publishStatusRoute,
  tablesRoute,
} from '@admin/routes/editReleaseRoutes';
import { Release } from '../../services/publicationService';

interface MatchProps {
  releaseId: string;
}

const EditReleaseContentPage = ({ match }: RouteComponentProps<MatchProps>) => {
  const { releaseId } = match.params;

  const [release, setRelease] = useState<Release>();

  const [publicationTitle, setPublicationTitle] = useState('');

  useEffect(() => {
    const selectedRelease = DummyPublicationsData.getReleaseById(releaseId);

    const owningPublication = DummyPublicationsData.getOwningPublicationForRelease(
      selectedRelease,
    );

    setRelease(selectedRelease);

    setPublicationTitle(owningPublication ? owningPublication.title : '');
  }, [releaseId]);

  return (
    <EditReleasePageTemplate
      publicationTitle={publicationTitle}
      releaseId={releaseId}
      previousLink={{
        label: tablesRoute.title,
        linkTo: tablesRoute.generateLink(releaseId),
      }}
      nextLink={{
        label: publishStatusRoute.title,
        linkTo: publishStatusRoute.generateLink(releaseId),
      }}
    >
      {release && <h2 className="govuk-heading-m">Add / edit content</h2>}
    </EditReleasePageTemplate>
  );
};

export default EditReleaseContentPage;
