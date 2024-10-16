
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using UnityEngine;

public class BuildingGenerator : MonoBehaviour
{
    public int seed = 0;
    Mesh mesh;
    private int grid_verts_per_side = 85;
   
    //make maps square bec of c# 2d array weirdness 
    
    private static int num_maps = 4;
    private int [][,] maps;
    private int grid_size = 10;
    GameObject roof_go;

    private enum direction {
        floor,
        left, right,  //vertical
        top, bottom  //horizontal
    }
    void Start()
    {
        Random.InitState(seed);
        mesh = GetComponent<MeshFilter>().mesh; //do not remove
        set_maps();
       

        HashSet<int> seen_maps = new HashSet<int>();

        for (int i = 0; i < 3; i++) {       //run three times to generate 3 buildings. 
            int building_offset = i * 100;
            int map_index = Random.Range(0, num_maps);
            while (seen_maps.Contains(map_index)) map_index = Random.Range(0, num_maps - 1);    //get new map
            seen_maps.Add(map_index); //add new map to seen list.
            int [,] selected_map = maps[map_index];

            // for (int row = 0; row < selected_map.GetLength(0); row++) {
            //     for (int col = 0; col < selected_map.GetLength(1); col++) {
            //         if (selected_map[row, col] != 0) {
            //             mesh_to_game_object(make_grid(grid_size, grid_size,  col * grid_size + building_offset, -row * grid_size, direction.floor));
            //             if (left_wall_possible(selected_map, row, col)) {
            //                 mesh_to_game_object(make_grid(grid_size, grid_size, col * grid_size + building_offset, -row * grid_size, direction.left));
            //             } 
            //             if (right_wall_possible(selected_map, row, col)) {
            //                 mesh_to_game_object(make_grid(grid_size, grid_size, (col + 1) * grid_size + building_offset, -row * grid_size, direction.right));
            //             }
            //             if (top_wall_possible(selected_map, row, col)) {
            //                 mesh_to_game_object(make_grid(grid_size, grid_size, col * grid_size + building_offset, - (row - 1) * grid_size, direction.top));
            //             } if (bottom_wall_possible(selected_map, row, col)) {
            //                 mesh_to_game_object(make_grid(grid_size, grid_size, col * grid_size + building_offset, - row * grid_size, direction.bottom));
            //             }

            //         }
            //     }
            // }
            // generate_building(selected_map, grid_size, building_offset);
            
        }

        //place windows and doors slightly offset
        // float offset = 0.01f;
        // Vector3[] verts = {new Vector3(1.5f,0,0), new Vector3(1.5f,0,1), new Vector3(2.5f,0,0)};    
        // for (int i = 0; i < verts.Length; i++) {
        //     verts[i] += new Vector3(offset, offset, offset);
        // }
        // int [] tris = {0, 1, 2};
        // Mesh m = new Mesh();
        // m.vertices = verts;
        // m.triangles = tris;
        // mesh_to_game_object(m);
        // mesh_to_game_object(make_grid(grid_size, grid_size, 0, 0, direction.top));
        // mesh_to_game_object(make_grid(grid_size, grid_size, 10, 10, direction.vertical));   

        // test_house();
       
        GameObject roof_1 = generate_cross_hip_roof(new Vector3(0, 0, 0), new Vector3(0,0,0));
        // GameObject roof_2 = generate_normal_roof();
        // roof_2.transform.Rotate(new Vector3(0, 90, 0));
        // roof_2.transform.Translate(new Vector3(-grid_size, 0, -grid_size / 2));
        // roof_go.transform.Translate(new Vector3(- grid_size  / 2, 0,  -grid_size / 2));
        // rotate_right(roof_1);
        rotate_left(roof_1);
        rotate_left(roof_1);
        
        
    }

    //generate a normal roof - basically a triangular prism. 
    GameObject generate_normal_roof(Vector3 rotate, Vector3 translate) {
        int height = grid_size / 2;
        int midpoint = grid_size / 2;
        Vector3[] verts = {
            //base 
            new Vector3(0, 0, 0), new Vector3(0, 0, grid_size), new Vector3(grid_size, 0, grid_size), new Vector3(grid_size, 0, 0),
            //front facing triangle
            new Vector3(0, 0, 0), new Vector3(midpoint, height, 0), new Vector3(grid_size, 0, 0),
            //back facing triangle
            new Vector3(0, 0, grid_size), new Vector3(grid_size, 0, grid_size), new Vector3(midpoint, height, grid_size), 
            //left quad
            new Vector3(0, 0, 0), new Vector3(0, 0, grid_size), new Vector3(midpoint, height, grid_size), new Vector3(midpoint, height, 0), 
            //right quad
            new Vector3(grid_size, 0, 0), new Vector3(midpoint, height, 0), new Vector3(grid_size, 0, grid_size), new Vector3(midpoint, height, grid_size),   
            
        };
        int[] tris = {
            //base 
            // 0, 1, 2, 0, 2, 3,
            2, 1, 0, 3, 2, 0,   //order so you see it from the bottom.
            //front facing triangle
            4, 5, 6,
            //back facing triangle
            7, 8, 9,
            //left quad
            10, 11, 12, 10, 12, 13,
            //right quad
            14, 15, 16, 15, 17, 16,



        };
        Mesh roof = new Mesh();
        roof.vertices = verts;
        roof.triangles = tris;

        roof.RecalculateNormals();
        GameObject s = new GameObject("roof");
        s.AddComponent<MeshFilter>();
        s.AddComponent<MeshRenderer>();
        
        // associate the mesh with this object
        s.GetComponent<MeshFilter>().mesh = roof;

        // change the color of the object
        Renderer rend = s.GetComponent<Renderer>();

        rend.material.color = Color.green;
        // s.transform.Rotate(new Vector3(0, 90, 0));
        roof_go = s;
        return s;
    }

    GameObject generate_cross_hip_roof(Vector3 rotate, Vector3 translate) {
        int height = grid_size / 2;
        int midpoint = grid_size / 2;
        Vector3[] verts = {
            //base 0-3
            new Vector3(0, 0, 0), new Vector3(0, 0, grid_size), new Vector3(grid_size, 0, grid_size), new Vector3(grid_size, 0, 0),
            //front facing triangle 4-6
            new Vector3(0, 0, 0), new Vector3(midpoint, height, 0), new Vector3(grid_size, 0, 0),
            
            //left quad 7-10 (please note that point 8 is extraneous... can remove at risk of breaking things but the optimzation isn't likely worth it)
            new Vector3(0, 0, 0), new Vector3(0, 0, midpoint), new Vector3(midpoint, height, midpoint), new Vector3(midpoint, height, 0), 
            // //right quad 11-14   
            new Vector3(grid_size, 0, 0), new Vector3(midpoint, height, 0), new Vector3(grid_size, 0, grid_size), new Vector3(midpoint, height, midpoint),   
            // left triangle 15-17
            new Vector3(0, 0, grid_size), new Vector3(0, height, midpoint), new Vector3(0, 0, 0),
            //connector triangle 18-20
            new Vector3(0,0,0), new Vector3(0, height, midpoint), new Vector3(midpoint, height, midpoint),
            //back wall 21-24
            new Vector3(grid_size, 0, grid_size), new Vector3(midpoint, height, midpoint), new Vector3(0, height, midpoint), new Vector3(0, 0, grid_size),
            
        };
        int[] tris = {
            //base 
            // 0, 1, 2, 0, 2, 3,
            2, 1, 0, 3, 2, 0,   //order so you see it from the bottom.
            //front facing triangle
            4, 5, 6,
            //left quad
            7, 9, 10,
            //right quad
            11, 12, 13, 12, 14, 13, 
            //left triangle
            15, 16, 17,
            //connector triangle
            18, 19, 20,
            //back wall
            21, 22, 23, 21, 23, 24,

        };
        Mesh roof = new Mesh();
        roof.vertices = verts;
        roof.triangles = tris;

        roof.RecalculateNormals();
        GameObject s = new GameObject("hip roof");
        s.AddComponent<MeshFilter>();
        s.AddComponent<MeshRenderer>();
        
        // associate the mesh with this object
        s.GetComponent<MeshFilter>().mesh = roof;

        // change the color of the object
        Renderer rend = s.GetComponent<Renderer>();

        rend.material.color = Color.green;
        // s.transform.Rotate(new Vector3(0, 90, 0));
        roof_go = s;
        return s;

    }

    GameObject generate_cross_gable_roof(Vector3 rotate, Vector3 translate) {
        int height = grid_size / 2;
        int midpoint = grid_size / 2;
        Vector3[] verts = {
            //base 0-3
            new Vector3(0, 0, 0), new Vector3(0, 0, grid_size), new Vector3(grid_size, 0, grid_size), new Vector3(grid_size, 0, 0),
            //front facing triangle 4-6
            new Vector3(0, 0, 0), new Vector3(midpoint, height, 0), new Vector3(grid_size, 0, 0),
            
            //left quad 7-10 (please note that point 8 is extraneous... can remove at risk of breaking things but the optimzation isn't likely worth it)
            new Vector3(0, 0, 0), new Vector3(0, 0, midpoint), new Vector3(midpoint, height, midpoint), new Vector3(midpoint, height, 0), 
            // //right quad 11-14 (please note point 14 is also extraneous)
            new Vector3(grid_size, 0, 0), new Vector3(grid_size, height, midpoint), new Vector3(grid_size, 0, grid_size), new Vector3(midpoint, height, midpoint),   
            // left triangle 15-17
            new Vector3(0, 0, grid_size), new Vector3(0, height, midpoint), new Vector3(0, 0, 0),
            //connector triangle 18-20
            new Vector3(0,0,0), new Vector3(0, height, midpoint), new Vector3(midpoint, height, midpoint),
            //back wall 21-24
            new Vector3(grid_size, 0, grid_size), new Vector3(grid_size, height, midpoint), new Vector3(0, height, midpoint), new Vector3(0, 0, grid_size),
            //connector triangle right 25-27
            new Vector3(grid_size, 0, 0), new Vector3(midpoint, height, 0), new Vector3(midpoint, height, midpoint),
            //connector triangle right 28-30
            new Vector3(grid_size, 0, 0), new Vector3(midpoint, height, midpoint), new Vector3(grid_size, height, midpoint),
            
        };
        int[] tris = {
            //base 
            // 0, 1, 2, 0, 2, 3,
            2, 1, 0, 3, 2, 0,   //order so you see it from the bottom.
            //front facing triangle
            4, 5, 6,
            //left quad
            7, 9, 10,
            //right quad
            // 11, 12, 13, 12, 14, 13, 
            11, 12, 13,
            //left triangle
            15, 16, 17,
            //connector triangle
            18, 19, 20,
            //back wall
            21, 22, 23, 21, 23, 24,
            //connector triangle right 
            25, 26, 27,
            //connector triangle right
            28, 29, 30,

        };
        Mesh roof = new Mesh();
        roof.vertices = verts;
        roof.triangles = tris;

        roof.RecalculateNormals();
        GameObject s = new GameObject("gable roof");
        s.AddComponent<MeshFilter>();
        s.AddComponent<MeshRenderer>();
        
        // associate the mesh with this object
        s.GetComponent<MeshFilter>().mesh = roof;

        // change the color of the object
        Renderer rend = s.GetComponent<Renderer>();

        rend.material.color = Color.green;
        // s.transform.Rotate(new Vector3(0, 90, 0));
        roof_go = s;
        return s;
    }

    //i've rotated twice in a row with no problem with these methods.
    void rotate_right(GameObject gm) {
        gm.transform.Rotate(new Vector3(0, 90, 0));
        gm.transform.Translate(new Vector3(-grid_size, 0, 0));
    }

    void rotate_left(GameObject gm) {
        gm.transform.Rotate(new Vector3(0, -90, 0));
        gm.transform.Translate(new Vector3(0, 0, -grid_size));
    }

    void generate_building(int [,] selected_map, int grid_size, int building_offset) {
        //loop for each floor.
        for (int row = 0; row < selected_map.GetLength(0); row++) {
                for (int col = 0; col < selected_map.GetLength(1); col++) {
                    if (selected_map[row, col] > 0) {  //maybe roll to see if next floor at this spot? then loop until see all zeroes. 
                    //generate random heights. then make walls that high in the positions. 
                    //generate random heights with max height. then run algo max_height times and subtract one from each nonzero spot.
                        mesh_to_game_object(make_grid(grid_size, grid_size,  col * grid_size + building_offset, -row * grid_size, direction.floor));
                        if (left_wall_possible(selected_map, row, col)) {
                            mesh_to_game_object(make_grid(grid_size, grid_size, col * grid_size + building_offset, -row * grid_size, direction.left));
                        } 
                        if (right_wall_possible(selected_map, row, col)) {
                            mesh_to_game_object(make_grid(grid_size, grid_size, (col + 1) * grid_size + building_offset, -row * grid_size, direction.right));
                        }
                        if (top_wall_possible(selected_map, row, col)) {
                            mesh_to_game_object(make_grid(grid_size, grid_size, col * grid_size + building_offset, - (row - 1) * grid_size, direction.top));
                        } if (bottom_wall_possible(selected_map, row, col)) {
                            mesh_to_game_object(make_grid(grid_size, grid_size, col * grid_size + building_offset, - row * grid_size, direction.bottom));
                        }
                        //selected_map[row, col]--;
                    }
                }
            }
    }
    bool left_wall_possible(int [,] map, int row, int col) {
        if (map[row, col] == 0) return false;
        //must be a floor at this cell.

        if (col == 0) return true;  //if leftmost and there is floor, then a left wall must happen.
        if (map[row, col - 1] == 0) return true;    //if there is not floor to the left, then put left floor
        return false;
    }

    bool right_wall_possible(int [,] map, int row, int col) {
        if (map[row, col] == 0) return false;
        //must be a floor at this cell.

        if (col == map.GetLength(1) - 1) return true;  //if rightmost and there is floor, then a right wall must happen.
        if (map[row, col + 1] == 0) return true;    //if there is not floor to the left, then put left floor
        return false;
    }
    
    bool top_wall_possible(int [,] map, int row, int col) {
        if (map[row, col] == 0) return false;
        //must be a floor at this cell.

        if (row == 0) return true;  //if leftmost and there is floor, then a left wall must happen.
        if (map[row - 1, col] == 0) return true;    //if there is not floor to the left, then put left floor
        return false;
    }

    bool bottom_wall_possible(int [,] map, int row, int col) {
        if (map[row, col] == 0) return false;
        //must be a floor at this cell.

        if (row == map.GetLength(0) - 1) return true;  //if rightmost and there is floor, then a right wall must happen.
        if (map[row + 1, col] == 0) return true;    //if there is not floor to the left, then put left floor
        return false;
    }
    
    
    void set_maps() {
        maps = new int[num_maps][,];
        int [,] map = new int[,] {
            {1, 1, 1}, 
            {0, 1, 0}, 
            {0, 0, 1}
        };

        int [,] map2 = new int[,] {
            {0, 0, 1, 0},
            {0, 1, 1, 0},
            {0, 1, 1, 1},
            {1, 1, 0, 1}
        };

        int [,] map3 = new int[,] {
            {1, 1, 1, 1, 1},
            {1, 0, 0, 0, 0},
            {1, 0, 0, 0, 0},
            {1, 0, 0, 0, 0},
            {1, 0, 0, 0, 0},
        };

        int [,] map4 = new int[,] {
            {1, 1, 1, 1, 1},
            {1, 0, 0, 0, 1},
            {1, 0, 0, 0, 1},
            {1, 0, 0, 0, 1},
            {1, 0, 0, 0, 1},
        };

        maps[0] = map;
        maps[1] = map2;
        maps[2] = map3;
        maps[3] = map4;
    }
    void OnDrawGizmos() {
        Gizmos.DrawSphere(new Vector3(0,0,0), 1);
        Gizmos.DrawSphere(new Vector3(10,0,10) , 0.5f);
        Gizmos.DrawSphere(new Vector3(9,0,9) , 0.5f);
        Gizmos.DrawSphere(new Vector3(8,0,8) , 0.5f);
        
        float grid_size = 10;
        float cell_size = 1;
        int steps = (int) (grid_size / cell_size);

        Vector3[] verts = new Vector3[(steps + 1) * (steps + 1)];
        for (int i = 0; i <= steps; i++) {
            for (int j = 0; j <= steps; j++) {
                float x_pos = cell_size * i; 
                float y_pos = cell_size * j;
                verts [i * steps + j] = new Vector3(x_pos, 0, y_pos);
                Gizmos.DrawSphere(verts[i * steps + j], 0.1f);
            }
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        // roof_go.transform.Rotate(new Vector3(0, -0.1f, 0));
        // roof_go.transform.Rotate(new Vector3(0, -0.1f, 0));
        // roof_go.transform.Translate(new Vector3( grid_size  / 2, grid_size / 2, 0));
        
        // roof_go.transform.RotateAround(new Vector3(grid_size / 2, grid_size / 2, 0),)
    }

    
    //must rewrite.
    //all are defined by bottom left corner and move in positive directions.
    //direction.floor = move in +x, +z
    //direction.horizontal = move in +x, +y
    //direciton.vertical = move in +y, +z
    //offsets move it in the xz plane
    private Mesh make_grid (int x_size, int y_size, float x_offset, float y_offset, direction d) {
		Mesh temp_mesh = new Mesh();
		

        Vector3[] verts = new Vector3[(x_size + 1) * (y_size + 1)];
		int vert_index = 0;
        for (int y = 0; y <= y_size; y++) {
			for (int x = 0; x <= x_size; x++) {
				if (d == direction.floor) {
                    verts[vert_index] = new Vector3(x + x_offset, 0, y + y_offset);   //rotate the verts based on ground, vertical, or horizontal wall
                } else if (d == direction.top || d == direction.bottom) {
                    verts[vert_index] = new Vector3(x + x_offset, y, y_offset);   //rotate the verts based on ground, vertical, or horizontal wall
                } else if (d == direction.left || d == direction.right) {
                    verts[vert_index] = new Vector3(x_offset, x , y + y_offset);   //rotate the verts based on ground, vertical, or horizontal wall
                }

                vert_index++;
			}
		}
		

		int[] tris = new int[x_size * y_size * 6];  //2 triangles per square, 3 points per triangle

        int triangle_index = 0, vertex_index = 0;
		for (int y = 0; y < y_size; y++) {
			for (int x = 0; x < x_size; x++) {
                //works for floor, bottom, and left
                //triangle 1:

                tris[triangle_index] = vertex_index;    //bottom left
                tris[triangle_index + 1] = vertex_index + x_size + 1;   //top left 
                tris[triangle_index + 2] = vertex_index + 1;    //bottom right

                //triangle 2:
				tris[triangle_index + 3] = vertex_index + 1;    //bottom right
				tris[triangle_index + 4] = vertex_index + x_size + 1;   //top left
				tris[triangle_index + 5] = vertex_index + x_size + 2;   //top right

                triangle_index += 6;    //added 6 verts
                vertex_index++;         //move to right
			}
            vertex_index++;             //move to next row
		}

        temp_mesh.vertices = verts;
        if (d == direction.top || d == direction.right) {
            System.Array.Reverse(tris);
        }
		temp_mesh.triangles = tris;
        return temp_mesh;
	}

    //color is determined here.
    GameObject mesh_to_game_object(Mesh mesh) {
        
        mesh.RecalculateNormals();
        GameObject s = new GameObject("terrain chunk");
        s.AddComponent<MeshFilter>();
        s.AddComponent<MeshRenderer>();
        
        // associate the mesh with this object
        s.GetComponent<MeshFilter>().mesh = mesh;

        // change the color of the object
        Renderer rend = s.GetComponent<Renderer>();

        rend.material.color = Color.blue;
        if (mesh.triangles.Length == 3) rend.material.color = Color.yellow;  //hack the coloring for "window" triangle.
        // color using Texture2D
        // if (terrain_selection == Terrain.Texture2D) {
        //     Texture2D texture = make_a_texture(mesh);
        //     rend.material.mainTexture = texture;
        // } else {
        //     get the renderer, attach a material that uses a vertex shader 
        //     thus, we can color each vertex and it mixes the colors. 
        //     note: this method is an alternative to using a texture 2D and potentially allows for a different gradient of colors to be made
        //     Material material = new Material(Shader.Find("Particles/Standard Surface"));
        //     rend.material = material;
        // }
       
        return s;
    }

    private Mesh create_mesh() {
        Mesh new_mesh = new Mesh();
        Vector3[] verts = new Vector3 [] {new Vector3(0,0,0), new Vector3(0,0,1), new Vector3(1,0,0), new Vector3(1,0,1)};
        int[] tris = new int [] {0 , 1, 2, 3, 2, 1};
        new_mesh.vertices = verts;
        new_mesh.triangles = tris;
        return new_mesh;
    }

    // make a triangle from three vertex indices (clockwise order)
	void MakeTri(int i1, int i2, int i3, int ntris, int [] tris) {
		int index = ntris * 3;  // figure out the base index for storing triangle indices

		tris[index]     = i1;
		tris[index + 1] = i2;
		tris[index + 2] = i3;
	}

	// make a quadrilateral from four vertex indices (clockwise order)
	void MakeQuad(int i1, int i2, int i3, int i4, int ntris, int[] tris) {
		MakeTri (i1, i2, i3, ntris, tris);
		MakeTri (i3, i4, i1, ntris + 1, tris);
	}
    
    private void display_mesh(Mesh new_mesh) {
        // mesh = GetComponent<MeshFilter>.mesh;
        mesh.Clear();
        mesh.vertices = new_mesh.vertices;
        mesh.triangles = new_mesh.triangles;
        mesh.RecalculateNormals();
    }  

    void test_house() {
        mesh_to_game_object(make_grid(grid_size, grid_size, 0, 0, direction.floor));
        mesh_to_game_object(make_grid(grid_size, grid_size, 0, 0, direction.bottom));
        mesh_to_game_object(make_grid(grid_size, grid_size, 0, 0, direction.left));
        mesh_to_game_object(make_grid(grid_size, grid_size, 0, grid_size, direction.top));
        mesh_to_game_object(make_grid(grid_size, grid_size, grid_size, 0, direction.right));        
    }  

    //generate a random_int from 0 to n - 1
    private int random_int(int n) {
        return (int) (Random.value * n);
    }

    //generate a random int from min to max - 1
    private int random_int(int min, int max) {
        return random_int(max - min) + min;
    }
}
