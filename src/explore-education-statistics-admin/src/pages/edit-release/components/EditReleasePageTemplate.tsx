import React, { ReactNode } from 'react';
import PreviousNextLinks, {
  PreviousNextLink,
} from '@admin/components/PreviousNextLinks';
import NavLink from '@admin/components/NavLink';
import Page from '../../../components/Page';
import editReleaseRoutes from '../../../routes/editReleaseRoutes';

interface Props {
  releaseId: string;
  children: ReactNode;
  publicationTitle: string;
  previousLink?: PreviousNextLink;
  nextLink?: PreviousNextLink;
}

const EditReleasePageTemplate = ({
  releaseId,
  publicationTitle,
  children,
  previousLink,
  nextLink,
}: Props) => {
  return (
    <Page
      wide
      breadcrumbs={[
        {
          link: '/admin-dashboard',
          name: 'Administrator dashboard',
        },
        { name: 'Edit release', link: '#' },
      ]}
    >
      <h1 className="govuk-heading-l">
        {publicationTitle}
        <span className="govuk-caption-l">Edit release</span>
      </h1>
      <nav className="app-navigation govuk-!-margin-bottom-9">
        <ul className="app-navigation__list govuk-!-margin-bottom-0">
          <li>
            {editReleaseRoutes.map(route => (
              <NavLink
                key={route.path}
                to={route.generateLink(releaseId)}
                activeClassName="app-navigation--current-page"
              >
                {route.title}
              </NavLink>
            ))}
          </li>
        </ul>
      </nav>
      {children}
      <PreviousNextLinks
        previousSection={previousLink}
        nextSection={nextLink}
      />
    </Page>
  );
};

export default EditReleasePageTemplate;
