import OrgLogoGov from '@common/components/OrgLogoGov';
import OrgLogoNonGov from '@common/components/OrgLogoNonGov';
import {
  defaultOrganisation,
  Organisation,
} from '@common/services/types/organisation';
import styles from '@frontend/modules/find-statistics/components/PublishingOrganisations.module.scss';

interface Props {
  publishingOrganisations?: Organisation[];
}

export default function PublishingOrganisations({
  publishingOrganisations,
}: Props) {
  let sortedOrganisations: Organisation[];
  if (!publishingOrganisations || publishingOrganisations?.length === 0) {
    sortedOrganisations = [defaultOrganisation];
  } else {
    sortedOrganisations = publishingOrganisations.sort((a, b) => {
      // DfE should always be first
      if (a.title === 'Department for Education') return -1;
      if (b.title === 'Department for Education') return 1;

      // Sort remaining alphabetically
      return a.title.localeCompare(b.title);
    });
  }

  return (
    <div className={`${styles.container} govuk-!-margin-bottom-6`}>
      {sortedOrganisations.map(organisation => {
        if (organisation.useGISLogo) {
          return (
            <OrgLogoGov
              key={organisation.id}
              crestFileName={organisation.logoFileName}
              lineColourHexCode={organisation.gisLogoHexCode ?? '#003764'}
              title={organisation.title}
            />
          );
        }
        return (
          <OrgLogoNonGov
            key={organisation.id}
            title={organisation.title}
            fileName={organisation.logoFileName}
          />
        );
      })}
    </div>
  );
}
