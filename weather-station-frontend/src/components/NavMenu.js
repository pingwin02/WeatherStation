import React from 'react';
import { Link } from 'react-router-dom';

function NavMenu() {
  return (
    <nav style={styles.navbar}>
      <ul style={styles.navList}>
        <li style={styles.navItem}>
          <Link to="/" style={styles.navLink}>Dashboard</Link>
        </li>
        <li style={styles.navItem}>
          <Link to="/sensors" style={styles.navLink}>Sensors</Link>
        </li>
        <li style={styles.navItem}>
          <Link to="/tokens" style={styles.navLink}>Token Balance</Link>
        </li>
      </ul>
    </nav>
  );
}

const styles = {
  navbar: {
    backgroundColor: '#333',
    padding: '1rem',
  },
  navList: {
    listStyleType: 'none',
    display: 'flex',
    gap: '1rem',
  },
  navItem: {
    margin: 0,
  },
  navLink: {
    color: 'white',
    textDecoration: 'none',
    fontSize: '1.2rem',
  },
};

export default NavMenu;
