import Tag from '@common/components/Tag';
import Link from '@admin/components/Link';
import TaskListItem from '@common/components/TaskListItem';
import React from 'react';

interface Props {
  id: string;
  isPatch: boolean;
  mappingComplete: boolean;
  majorChanges: boolean;
  mappingPageRoute: string;
  mappingText: string;
  mappingHintText: string;
  showRejectedError: boolean;
}

export default function DataSetMappingTaskListItem({
  id,
  isPatch,
  mappingComplete,
  majorChanges,
  mappingPageRoute,
  mappingText,
  mappingHintText,
  showRejectedError,
}: Props) {
  const warning = mappingComplete && isPatch ? majorChanges : !mappingComplete;

  return (
    <TaskListItem
      id={id}
      status={
        <Tag colour={warning ? 'red' : 'blue'}>
          {mappingComplete
            ? getMappingCompleteText(showRejectedError, majorChanges)
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
