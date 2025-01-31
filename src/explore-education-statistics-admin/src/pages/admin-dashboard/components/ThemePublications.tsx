import ButtonLink from '@admin/components/ButtonLink';
import styles from '@admin/pages/admin-dashboard/components/ThemePublications.module.scss';
import { publicationCreateRoute, ThemeParams } from '@admin/routes/routes';
import Link from '@admin/components/Link';
import {
  publicationReleasesRoute,
  PublicationRouteParams,
} from '@admin/routes/publicationRoutes';
import permissionService from '@admin/services/permissionService';
import publicationService from '@admin/services/publicationService';
import { Theme } from '@admin/services/themeService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import orderBy from 'lodash/orderBy';
import React from 'react';
import { generatePath } from 'react-router';

interface Props {
  theme: Theme;
}

const ThemePublications = ({ theme }: Props) => {
  const { value, isLoading } = useAsyncHandledRetry(async () => {
    const [publications, canCreatePublication] = await Promise.all([
      publicationService.listPublications(theme.id),
      permissionService.canCreatePublicationForTheme(theme.id),
    ]);
    return { publications, canCreatePublication };
  });

  const { publications, canCreatePublication } = value ?? {};

  return (
    <div className={styles.publication} data-testid="theme-publications">
      <div data-testid={`theme-publications-${theme.title}`}>
        <h3>{theme.title}</h3>
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
          to={generatePath<ThemeParams>(publicationCreateRoute.path, {
            themeId: theme.id,
          })}
        >
          Create new publication
        </ButtonLink>
      )}
    </div>
  );
};

export default ThemePublications;
