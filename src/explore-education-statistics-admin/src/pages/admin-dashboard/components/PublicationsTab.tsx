import FormThemeSelect from '@admin/components/form/FormThemeSelect';
import useQueryParams from '@admin/hooks/useQueryParams';
import ThemePublications from '@admin/pages/admin-dashboard/components/ThemePublications';
import { ThemeParams, dashboardRoute } from '@admin/routes/routes';
import themeService, { Theme } from '@admin/services/themeService';
import appendQuery from '@common/utils/url/appendQuery';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import useStorageItem from '@common/hooks/useStorageItem';
import orderBy from 'lodash/orderBy';
import React, { useEffect, useMemo } from 'react';
import { useHistory, useLocation } from 'react-router';

interface Props {
  isBauUser: boolean;
}

const PublicationsTab = ({ isBauUser }: Props) => {
  const { themeId, topicId } = useQueryParams<ThemeTopicParams>();
  const location = useLocation();
  const history = useHistory();

  const [savedTheme, setSavedTheme] = useStorageItem<ThemeParams>(
    'dashboardTheme',
    undefined,
  );

  const { value: themes, isLoading: loadingThemes } = useAsyncHandledRetry(
    themeService.getThemes,
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

    // Set default theme/topic in storage if it hasn't been set yet (e.g. first time
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
    // theme/topic if they haven't already been set.
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
    topicId,
  ]);

  return (
    <LoadingSpinner
      hideText
      loading={loadingThemes}
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
            <FormThemeSelect
              id="publicationsReleases-themeTopic"
              legend="Choose a theme and topic to view publications for"
              legendHidden
              themes={themes}
              topicId={topicId}
              onChange={nextThemeId => {
                setSavedTheme({
                  themeId: nextThemeId,
                });

                history.replace(
                  appendQuery<ThemeParams>(dashboardRoute.path, {
                    themeId: nextThemeId,
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
            <ThemePublications
              key={selectedTheme.id}
              themeTitle={selectedTheme.title}
              theme={selectedTheme}
            />
          ) : (
            <>
              {themes?.map(theme => {
                return (
                  <ThemePublications
                    key={theme.id}
                    themeTitle={theme.title}
                    theme={theme}
                  />
                );
              })}
            </>
          )}
        </>
      )}
    </LoadingSpinner>
  );
};

export default PublicationsTab;
