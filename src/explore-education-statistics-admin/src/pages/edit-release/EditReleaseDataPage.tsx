import React, { useEffect, useState } from 'react';
import { RouteComponentProps } from 'react-router';
import DummyPublicationsData from '@admin/pages/DummyPublicationsData';
import EditReleasePageTemplate from '@admin/pages/edit-release/components/EditReleasePageTemplate';
import {
  buildTablesRoute,
  dataRoute,
  setupRoute,
} from '@admin/routes/editReleaseRoutes';
import { Release } from '../../services/publicationService';

interface MatchProps {
  releaseId: string;
}

const EditReleaseDataPage = ({ match }: RouteComponentProps<MatchProps>) => {
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
        label: setupRoute.title,
        linkTo: setupRoute.generateLink(releaseId),
      }}
      nextLink={{
        label: buildTablesRoute.title,
        linkTo: buildTablesRoute.generateLink(releaseId),
      }}
    >
      {release && <h2 className="govuk-heading-m">Add / edit data</h2>}
    </EditReleasePageTemplate>
  );
};

export default EditReleaseDataPage;
