import Tag from '@common/components/Tag';
import Link from '@admin/components/Link';
import TaskListItem from '@common/components/TaskListItem';
import React from 'react';

interface Props {
  id: string;
  isPatch: boolean;
  mappingCompleteForFacet: boolean;
  majorChangesForFacet: boolean;
  mappingPageRoute: string;
  mappingText: string;
  mappingHintText: string;
  majorVersionRejected: boolean;
}

export default function DataSetMappingTaskListItem({
  id,
  isPatch,
  mappingCompleteForFacet,
  majorChangesForFacet,
  mappingPageRoute,
  mappingText,
  mappingHintText,
  majorVersionRejected,
}: Props) {
  const warning =
    mappingCompleteForFacet && isPatch
      ? majorChangesForFacet
      : !mappingCompleteForFacet;

  return (
    <TaskListItem
      id={id}
      status={
        <Tag colour={warning ? 'red' : 'blue'}>
          {mappingCompleteForFacet
            ? getMappingCompleteText(majorVersionRejected, majorChangesForFacet)
            : 'Incomplete'}
        </Tag>
      }
      hint={mappingHintText}
    >
      {props => (
        <Link {...props} to={mappingPageRoute}>
          {mappingText}
        </Link>
      )}
    </TaskListItem>
  );
}

function getMappingCompleteText(
  shouldShowErrorOnMajorVersion: boolean,
  majorVersionFound: boolean,
): React.ReactNode {
  return shouldShowErrorOnMajorVersion && majorVersionFound
    ? 'Major Change'
    : 'Complete';
}
