import LoginContext from '@admin/components/Login';
import Page from '@admin/components/Page';
import PublicationReleaseContent from '@admin/modules/find-statistics/PublicationReleaseContent';
import { releaseContentService } from '@admin/services/release/edit-release/content/service';
import { ManageContentPageViewModel } from '@admin/services/release/edit-release/content/types';
import withErrorControl, {
  ErrorControlProps,
} from '@admin/validation/withErrorControl';
import React, { useContext, useEffect, useState } from 'react';
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

  const { user } = useContext(LoginContext);

  const { releaseId } = match.params;

  useEffect(() => {
    releaseContentService
      .getContent(releaseId)
      .then(content => {
        const newContent = {
          ...content,
          release: {
            ...content.release,
            prerelease: true,
          },
        };

        setModel({
          content: newContent,
        });
      })
      .catch(handleApiErrors);
  }, [releaseId, handleApiErrors]);

  return (
    <>
      {model && (
        <Page
          wide
          breadcrumbs={
            user && user.permissions.canAccessAnalystPages
              ? [{ name: 'View pre release' }]
              : []
          }
        >
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
