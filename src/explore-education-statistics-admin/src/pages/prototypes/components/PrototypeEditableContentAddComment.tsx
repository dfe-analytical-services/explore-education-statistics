import React from 'react';
import Details from '@common/components/Details';
import {
  FormGroup,
  FormTextInput,
  FormSelect,
  FormRadioGroup,
} from '@common/components/form';

import { LoginContext } from '@admin/components/Login';
import { User } from '@admin/services/PrototypeLoginService';
import styles from './PrototypeEditableContentAddComment.module.scss';

const key = (() => {
  let keyValue = 0;
  return () => {
    // eslint-disable-next-line no-plusplus
    keyValue++;
    return keyValue;
  };
})();

interface Comment {
  name: string;
  time: Date;
  comment: string;
}

const ContentAddComment = () => {
  const [currentComment, setCurrentComment] = React.useState<string>('');
  const [comments, setComments] = React.useState<Comment[]>([]);

  const context = React.useContext(LoginContext);

  const addComment = (comment: string) => {
    const user: User = context.user || {
      name: 'guest',
      id: 'guest',
      permissions: [],
    };

    setComments([...comments, { name: user.name, time: new Date(), comment }]);
    setCurrentComment('');
  };

  const removeComment = (index: number) => {
    comments.splice(index, 1);
    setComments([...comments]);
  };

  return (
    <>
      <div className={styles.addComment}>
        <Details summary="Add comment" className="govuk-!-margin-bottom-1">
          {comments.map(({ name, comment, time }, index) => (
            <div key={key()}>
              <h2 className="govuk-body-xs govuk-!-margin-0">
                <strong>{`${name} ${time.toLocaleDateString()}`}</strong>
              </h2>
              <p className="govuk-body-xs govuk-!-margin-bottom-1 ">
                {comment}
              </p>
              {/* eslint-disable-next-line jsx-a11y/click-events-have-key-events */}
              <a
                className="govuk-body-xs"
                role="button"
                tabIndex={0}
                onClick={() => removeComment(index)}
                style={{ cursor: 'pointer' }}
              >
                Remove
              </a>
              <hr />
            </div>
          ))}

          <form>
            <textarea
              name="comment"
              id="comment"
              defaultValue=""
              value={currentComment}
              onChange={e => setCurrentComment(e.target.value)}
            />
            <button
              type="button"
              className="govuk-button"
              disabled={currentComment.length === 0}
              onClick={() => {
                addComment(currentComment);
              }}
            >
              Submit
            </button>
          </form>
        </Details>
      </div>
    </>
  );
};

export default ContentAddComment;
