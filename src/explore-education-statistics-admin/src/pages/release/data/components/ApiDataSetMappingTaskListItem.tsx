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
  taskText: string;
  taskHintText: string;
}

export default function ApiDataSetMappingTaskListItem({
  id,
  isPatch,
  mappingCompleteForFacet,
  majorChangesForFacet,
  mappingPageRoute,
  taskText,
  taskHintText,
}: Props) {
  const warning = isWarningRequired(
    mappingCompleteForFacet,
    isPatch,
    majorChangesForFacet,
  );
  return (
    <TaskListItem
      id={id}
      status={
        <Tag colour={warning ? 'red' : 'blue'} testId={`${id}-tag`}>
          {mappingCompleteForFacet
            ? getMappingCompleteText(isPatch, majorChangesForFacet)
            : 'Incomplete'}
        </Tag>
      }
      hint={taskHintText}
    >
      {props => (
        <Link {...props} to={mappingPageRoute}>
          {taskText}
        </Link>
      )}
    </TaskListItem>
  );
}

function isWarningRequired(
  mappingCompleteForFacet: boolean,
  isPatch: boolean,
  majorChangesForFacet: boolean,
) {
  // If mapping work is still required, display a warning style.
  if (!mappingCompleteForFacet) {
    return true;
  }

  // If mapping work is complete, but this is a patch replacement and the mapping work for this
  // particular facet has resulted in a major change (which is disallowed for patch replacements),
  // display a warning style.
  if (mappingCompleteForFacet && isPatch) {
    return majorChangesForFacet;
  }

  // Otherwise, display a success style.
  return false;
}

function getMappingCompleteText(
  isPatch: boolean,
  majorVersionFound: boolean,
): React.ReactNode {
  return isPatch && majorVersionFound ? 'Major Change' : 'Complete';
}
