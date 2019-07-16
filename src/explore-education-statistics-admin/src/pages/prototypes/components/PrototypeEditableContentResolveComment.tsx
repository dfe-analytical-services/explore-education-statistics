/* eslint-disable no-shadow,jsx-a11y/click-events-have-key-events,jsx-a11y/click-events-have-key-events */
import React from 'react';
import Details from '@common/components/Details';

import { LoginContext } from '@admin/components/Login';
import { Comment } from './PrototypeEditableContentAddComment';

import styles from './PrototypeEditableContentAddComment.module.scss';

interface Props {
  name?: string;
  date?: Date;
}

const key = (() => {
  let keyValue = 0;
  return () => {
    // eslint-disable-next-line no-plusplus
    keyValue++;
    return keyValue;
  };
})();

const ContentResolveComment = ({ name, date }: Props) => {
  const context = React.useContext(LoginContext);

  const [comments, setComments] = React.useState<Comment[]>([
    {
      name: name || 'guest',
      time: date || new Date(),
      comment: `Lorem ipsum dolor sit, amet consectetur adipisicing elit. Tempore,
              nostrum atque assumenda vitae quae ratione nihil accusamus
              provident natus ipsum iure numquam ex labore nam, deleniti impedit
              repudiandae eligendi quasi.`,
      state: 'open',
    },
  ]);

  const resolveComment = (index: number) => {
    comments[index].state = 'resolved';
    comments[index].resolvedOn = new Date();
    comments[index].resolvedBy = context.user && context.user.name;
    setComments([...comments]);
  };

  return (
    <>
      <div className={styles.resolveComment}>
        <Details summary="View comments" className="govuk-!-margin-bottom-1">
          {comments.map(
            ({ name, time, comment, state, resolvedOn, resolvedBy }, index) => (
              <div key={key()}>
                <h2 className="govuk-body-xs govuk-!-margin-0">
                  <strong>{`${name} ${time.toLocaleDateString()}`}</strong>
                </h2>
                <p className="govuk-body-xs govuk-!-margin-bottom-1 ">
                  {comment}
                </p>
                {state === 'open' && (
                  <button
                    type="button"
                    className="govuk-button"
                    onClick={() => resolveComment(index)}
                  >
                    Resolve
                  </button>
                )}
                {state === 'resolved' && (
                  <p className="govuk-body-xs govuk-!-margin-bottom-1 ">
                    <em>
                      Resolved {resolvedOn && resolvedOn.toLocaleDateString()}{' '}
                      by {resolvedBy}
                    </em>
                  </p>
                )}
                <hr />
              </div>
            ),
          )}
        </Details>
      </div>
    </>
  );
};

export default ContentResolveComment;
