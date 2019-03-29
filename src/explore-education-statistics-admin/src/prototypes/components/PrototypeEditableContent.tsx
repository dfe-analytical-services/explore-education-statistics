import * as React from "react";
// @ts-ignore
import CKEditor from '@ckeditor/ckeditor5-react';
// @ts-ignore
import Editor from '@ckeditor/ckeditor5-build-balloon-block';
import {ReactComponentLike, ReactNodeLike} from "prop-types";
import {Fragment} from "react";

// import 'react-draft-wysiwyg/dist/react-draft-wysiwyg.css';

interface Props {
  content: string;
}

interface State {
  editing: boolean;
}

export class PrototypeEditableContent extends React.Component<Props, State> {

  private ref: HTMLElement | null = null;

  public state: State = {
    editing: false
  };

  componentDidMount() {
    if (!this.state.editing && this.ref) {
      this.ref.innerHTML = this.props.content;
    }
  }

  componentDidUpdate() {
    if (!this.state.editing && this.ref) {
      this.ref.style.backgroundColor='';
      this.ref.innerHTML = this.props.content;
    }
  }

  setEditing = (event : React.MouseEvent<HTMLElement>) => {
    console.log(event);
    event.preventDefault();
    this.setState({editing: true})
  };

  save = () => {

    this.setState({editing: false})
  };

  public render() {

    return (
      <Fragment>
        {this.state.editing ?
          <Fragment>
            <CKEditor
              editor={Editor}
              data={this.props.content}
            />
            <button onClick={ this.save }>Save</button>
          </Fragment>
          : <Fragment>
            <div ref={ref => this.ref = ref} />
            <button onClick={ event => this.setEditing(event) }>Edit</button>
          </Fragment>
        }
      </Fragment>
    );
  }

}