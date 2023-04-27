import React from 'react';
import ButtonLink from '@admin/components/ButtonLink';
import styles from '@admin/pages/admin-dashboard/components/TopicPublications.module.scss';
import { publicationCreateRoute, TopicParams } from '@admin/routes/routes';
import Link from '@admin/components/Link';
import {
  publicationReleasesRoute,
  PublicationRouteParams,
} from '@admin/routes/publicationRoutes';
import permissionService from '@admin/services/permissionService';
import publicationService from '@admin/services/publicationService';
import { Topic } from '@admin/services/topicService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import orderBy from 'lodash/orderBy';
import { generatePath } from 'react-router';

interface Props {
  themeTitle: string;
  topic: Topic;
}

const TopicPublications = ({ topic, themeTitle }: Props) => {
  const { value, isLoading } = useAsyncHandledRetry(async () => {
    const [publications, canCreatePublication] = await Promise.all([
      publicationService.listPublications(topic.id),
      permissionService.canCreatePublicationForTopic(topic.id),
    ]);
    return { publications, canCreatePublication };
  });

  const { publications, canCreatePublication } = value ?? {};

  return (
    <div className={styles.publication} data-testid="topic-publications">
      <div data-testid={`topic-publications-${themeTitle}-${topic.title}`}>
        <h3>{`${themeTitle} / ${topic.title}`}</h3>
        <LoadingSpinner
          hideText
          inline
          loading={isLoading}
          text="Loading publications"
          size="md"
        >
          {publications?.length !== 0 ? (
            <ul className="govuk-list">
              {orderBy(publications, 'title')?.map(publication => (
                <li key={publication.id}>
                  <Link
                    to={generatePath<PublicationRouteParams>(
                      publicationReleasesRoute.path,
                      {
                        publicationId: publication.id,
                      },
                    )}
                  >
                    {publication.title}
                  </Link>
                </li>
              ))}
            </ul>
          ) : (
            <p>No publications available</p>
          )}
        </LoadingSpinner>
      </div>
      {canCreatePublication && (
        <ButtonLink
          to={generatePath<TopicParams>(publicationCreateRoute.path, {
            topicId: topic.id,
          })}
        >
          Create new publication
        </ButtonLink>
      )}
    </div>
  );
};

export default TopicPublications;
