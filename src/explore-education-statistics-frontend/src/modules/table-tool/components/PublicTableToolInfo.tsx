import TableToolInfo from '@common/modules/table-tool/components/TableToolInfo';
import { SelectedPublication } from '@common/modules/table-tool/types/selectedPublication';
import { ReleaseVersion } from '@common/services/publicationService';
import Link from '@frontend/components/Link';
import React, { ReactNode } from 'react';

interface Props {
  selectedPublication?: SelectedPublication;
  fullPublication?: ReleaseVersion;
}

export default function PublicTableToolInfo({
  selectedPublication,
  fullPublication,
}: Props) {
  if (!selectedPublication || !fullPublication) return null;

  const { publication } = fullPublication;

  const getMethodologyLinks = () => {
    const links: ReactNode[] =
      publication?.methodologies?.map(methodology => (
        <Link key={methodology.id} to={`/methodology/${methodology.slug}`}>
          {methodology.title}
        </Link>
      )) ?? [];

    if (publication?.externalMethodology) {
      links.push(
        <Link
          key={publication.externalMethodology.url}
          to={publication.externalMethodology.url}
        >
          {publication.externalMethodology.title}
        </Link>,
      );
    }
    return links;
  };

  return (
    <TableToolInfo
      contactDetails={publication?.contact}
      methodologyLinks={getMethodologyLinks()}
      releaseLink={
        <Link
          to={`/find-statistics/${selectedPublication.slug}/${selectedPublication.latestRelease.slug}`}
        >
          {`${selectedPublication.title}, ${selectedPublication.latestRelease.title}`}
        </Link>
      }
      releaseType={selectedPublication.selectedRelease.type}
    />
  );
}
