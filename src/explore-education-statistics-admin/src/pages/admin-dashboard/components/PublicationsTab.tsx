import useQueryParams from '@admin/hooks/useQueryParams';
import ThemePublications from '@admin/pages/admin-dashboard/components/ThemePublications';
import { ThemeParams, dashboardRoute } from '@admin/routes/routes';
import { Theme } from '@admin/services/themeService';
import appendQuery from '@common/utils/url/appendQuery';
import LoadingSpinner from '@common/components/LoadingSpinner';
import themeQueries from '@admin/queries/themeQueries';
import useStorageItem from '@common/hooks/useStorageItem';
import orderBy from 'lodash/orderBy';
import React, { useEffect, useMemo } from 'react';
import { useHistory, useLocation } from 'react-router';
import { useQuery } from '@tanstack/react-query';
import { FormSelect } from '@common/components/form';

interface Props {
  isBauUser: boolean;
}

const PublicationsTab = ({ isBauUser }: Props) => {
  const { themeId } = useQueryParams<ThemeParams>();
  const location = useLocation();
  const history = useHistory();

  const [savedTheme, setSavedTheme] = useStorageItem<ThemeParams>(
    'dashboardTheme',
    undefined,
  );

  const { data: themes, isLoading: isLoadingThemes } = useQuery(
    themeQueries.listThemes,
  );

  const selectedTheme = useMemo<Theme | undefined>(() => {
    // Only select a theme for BAU users.
    // Analysts see all their publications for all themes.
    if (!isBauUser) {
      return undefined;
    }
    const selectedThemeId = themeId || savedTheme?.themeId;

    return (
      themes?.find(t => t.id === selectedThemeId) ??
      orderBy(themes, t => t.title)[0]
    );
  }, [isBauUser, savedTheme, themeId, themes]);

  useEffect(() => {
    if (savedTheme) {
      return;
    }

    // Set default theme in storage if it hasn't been set yet (e.g. first time
    // visiting dashboard).
    if (selectedTheme) {
      setSavedTheme({
        themeId: selectedTheme.id,
      });
    }
  }, [isBauUser, savedTheme, selectedTheme, setSavedTheme]);

  useEffect(() => {
    if (!selectedTheme) {
      return;
    }

    // Update query params to reflect the chosen
    // theme if they haven't already been set.
    if (selectedTheme?.id !== themeId) {
      history.replace(
        appendQuery<ThemeParams>(location.pathname, {
          themeId: selectedTheme?.id,
        }),
      );
    }
  }, [
    isBauUser,
    history,
    location.pathname,
    savedTheme,
    selectedTheme,
    themeId,
  ]);

  return (
    <LoadingSpinner
      hideText
      loading={isLoadingThemes}
      text="Loading your publications"
    >
      <h2>View and manage your publications</h2>
      <p>Select a publication to:</p>

      <ul>
        <li>create new releases and methodologies</li>
        <li>edit exiting releases and methodologies</li>
        <li>view and sign-off releases and methodologies</li>
      </ul>

      {isBauUser && (
        <>
          {themes && themes.length > 0 && (
            <FormSelect
              className="govuk-!-width-one-half"
              id="publicationsReleases-theme"
              inline={false}
              label="Select theme"
              name="themeId"
              value={themeId}
              options={themes.map(theme => {
                return {
                  label: theme.title,
                  value: theme.id,
                };
              })}
              onChange={e => {
                setSavedTheme({
                  themeId: e.target.value,
                });

                history.replace(
                  appendQuery<ThemeParams>(dashboardRoute.path, {
                    themeId: e.target.value,
                  }),
                );
              }}
            />
          )}
        </>
      )}

      <hr className="govuk-!-margin-bottom-0" />

      {themes?.length === 0 ? (
        <>
          <h3
            className="govuk-heading-s"
            data-testid="no-permission-to-access-releases"
          >
            You do not currently have permission to edit any releases within the
            service. To view a prerelease, please use the link provided to you
            by email.
          </h3>
          <p>
            To request access to a release, contact your team leader or the
            Explore education statistics team at{' '}
            <a href="mailto:explore.statistics@education.gov.uk">
              explore.statistics@education.gov.uk
            </a>
          </p>
        </>
      ) : (
        <>
          {selectedTheme ? (
            <ThemePublications key={selectedTheme.id} theme={selectedTheme} />
          ) : (
            <>
              {themes?.map(theme => {
                return <ThemePublications key={theme.id} theme={theme} />;
              })}
            </>
          )}
        </>
      )}
    </LoadingSpinner>
  );
};

export default PublicationsTab;
