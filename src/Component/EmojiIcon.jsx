// src/components/EmojiIcon.jsx
import React from 'react';
import { combinePath } from '../Tools/tools';
import { BaseURLFiles } from '../api/apiNeed';

/**
 * Displays a single emoji PNG from the Emoji-pack assets.
 * @param {string} name - the png filename (without extension), e.g. "smile"
 * @param {string} className - optional extra CSS classes
 * @param {object} style - optional inline styles
 */
const EmojiIcon = ({ name, className = "", style = {} }) => {
  // Build the correct asset path. Adjust the base path if needed.
  const basePath = `/img/rooms/icons/${name}.png`;
  const src = combinePath(BaseURLFiles, `${basePath}`);

  return (
    <>
      {name && (
        <img
          src={src}
          alt={name}
          className={`emoji-icon inline-block ${className}`}
          style={{ width: "1.5em", height: "1.5em", ...style }}
        />
      )}
    </>
  )
}

export default EmojiIcon;