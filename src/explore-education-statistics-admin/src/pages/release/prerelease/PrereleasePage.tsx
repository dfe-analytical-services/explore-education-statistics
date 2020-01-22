import Link from '@admin/components/Link';
import Page from '@admin/components/Page';
import PublicationReleaseContent from '@admin/modules/find-statistics/PublicationReleaseContent';
import { releaseContentService } from '@admin/services/release/edit-release/content/service';
import { ManageContentPageViewModel } from '@admin/services/release/edit-release/content/types';
import withErrorControl, {
  ErrorControlProps,
} from '@admin/validation/withErrorControl';
import RelatedInformation from '@common/components/RelatedInformation';
import React, { useEffect, useState } from 'react';
import { RouteComponentProps, withRouter } from 'react-router';

interface Model {
  content: ManageContentPageViewModel;
}

interface MatchProps {
  releaseId: string;
}

const ReleaseContentPage = ({
  handleApiErrors,
  match,
}: RouteComponentProps<MatchProps> & ErrorControlProps) => {
  const [model, setModel] = useState<Model>();

  const { releaseId } = match.params;

  useEffect(() => {
    Promise.all([releaseContentService.getContent(releaseId)])
      .then(([newContent]) => {
        setModel({
          content: newContent,
        });
      })
      .catch(handleApiErrors);
  }, [releaseId, handleApiErrors]);

  return (
    <>
      {model && (
        <Page wide breadcrumbs={[{ name: 'View prerelease' }]}>
          <PublicationReleaseContent
            editing={false}
            content={model.content}
            styles={{}}
            onReleaseChange={_ => {}}
            availableDataBlocks={[]}
          />
        </Page>
      )}
    </>
  );
};

export default withErrorControl(withRouter(ReleaseContentPage));
