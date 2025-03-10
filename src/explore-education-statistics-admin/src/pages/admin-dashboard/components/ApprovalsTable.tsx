import Link from '@admin/components/Link';
import { MethodologyVersion } from '@admin/services/methodologyService';
import { DashboardReleaseVersionSummary } from '@admin/services/releaseVersionService';
import {
  MethodologyRouteParams,
  methodologyContentRoute,
} from '@admin/routes/methodologyRoutes';
import {
  ReleaseRouteParams,
  releaseContentRoute,
} from '@admin/routes/releaseRoutes';
import VisuallyHidden from '@common/components/VisuallyHidden';
import { Dictionary } from '@common/types';
import orderBy from 'lodash/orderBy';
import React, { useMemo } from 'react';
import { generatePath } from 'react-router';
import merge from 'lodash/merge';

interface Props {
  methodologyApprovals: MethodologyVersion[];
  releaseApprovals: DashboardReleaseVersionSummary[];
}

export default function ApprovalsTable({
  methodologyApprovals,
  releaseApprovals,
}: Props) {
  const releasesByPublication: Dictionary<{
    releases: DashboardReleaseVersionSummary[];
  }> = useMemo(() => {
    return releaseApprovals.reduce<
      Dictionary<{ releases: DashboardReleaseVersionSummary[] }>
    >((acc, releaseVersion) => {
      if (acc[releaseVersion.publication.title]) {
        acc[releaseVersion.publication.title].releases.push(releaseVersion);
      } else {
        acc[releaseVersion.publication.title] = {
          ...acc[releaseVersion.publication.title],
          releases: [releaseVersion],
        };
      }
      return acc;
    }, {});
  }, [releaseApprovals]);

  const methodologiesByPublication: Dictionary<{
    methodologies: MethodologyVersion[];
  }> = useMemo(() => {
    return methodologyApprovals.reduce<
      Dictionary<{ methodologies: MethodologyVersion[] }>
    >((acc, methodology) => {
      if (acc[methodology.owningPublication.title]) {
        acc[methodology.owningPublication.title].methodologies.push(
          methodology,
        );
      } else {
        acc[methodology.owningPublication.title] = {
          methodologies: [methodology],
        };
      }
      return acc;
    }, {});
  }, [methodologyApprovals]);

  const allApprovalsByPublication = merge(
    releasesByPublication,
    methodologiesByPublication,
  );

  const publications = Object.keys(allApprovalsByPublication);

  if (!publications.length) {
    return (
      <p>There are no releases or methodologies awaiting your approval.</p>
    );
  }

  return (
    <>
      <p>Here you can view any releases or methodologies awaiting approval.</p>
      <table data-testid="your-approvals">
        <thead>
          <tr>
            <th>Publication / Page</th>
            <th>Page type</th>
            <th>Actions</th>
          </tr>
          {orderBy(publications).map(publication => (
            <PublicationRow
              key={publication}
              publication={publication}
              methodologies={
                allApprovalsByPublication[publication].methodologies
              }
              releases={allApprovalsByPublication[publication].releases}
            />
          ))}
        </thead>
      </table>
    </>
  );
}

interface PublicationRowProps {
  publication: string;
  methodologies: MethodologyVersion[];
  releases: DashboardReleaseVersionSummary[];
}

function PublicationRow({
  publication,
  methodologies,
  releases,
}: PublicationRowProps) {
  return (
    <>
      <tr key={publication}>
        <th className="govuk-!-padding-top-6" colSpan={3}>
          {publication}
        </th>
      </tr>
      {releases?.map(releaseVersion => (
        <tr
          key={releaseVersion.id}
          data-testid={`release-${publication} - ${releaseVersion.title}`}
        >
          <td>{releaseVersion.title}</td>
          <td>Release</td>
          <td>
            <Link
              to={generatePath<ReleaseRouteParams>(releaseContentRoute.path, {
                publicationId: releaseVersion.publication.id,
                releaseVersionId: releaseVersion.id,
              })}
            >
              Review this page
              <VisuallyHidden> for {releaseVersion.title}</VisuallyHidden>
            </Link>
          </td>
        </tr>
      ))}
      {methodologies?.map(methodology => (
        <tr
          key={methodology.id}
          data-testid={`methodology-${publication} - ${methodology.title}`}
        >
          <td>{methodology.title}</td>
          <td>Methodology</td>
          <td>
            <Link
              to={generatePath<MethodologyRouteParams>(
                methodologyContentRoute.path,
                {
                  methodologyId: methodology.id,
                },
              )}
            >
              Review this page
              <VisuallyHidden> for {methodology.title}</VisuallyHidden>
            </Link>
          </td>
        </tr>
      ))}
    </>
  );
}
