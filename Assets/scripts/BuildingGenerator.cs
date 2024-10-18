
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using UnityEngine;

public class BuildingGenerator : MonoBehaviour
{
    public int seed = 0;
    Mesh mesh;
   
    //make maps square bec of c# 2d array weirdness 
    
    private static int num_maps = 9;
    private int [][,] maps;
    private int grid_size = 10;
    GameObject roof_go;
    IntArrayEqualityComparer int_comp = new IntArrayEqualityComparer();
    Dictionary<int[], GameObject> neighbors_to_roof;

    private enum direction {
        floor,
        left, right,  //vertical
        top, bottom  //horizontal
    }

    private enum window_type {
        square,
        diamond,
    }

    private enum door_type {
        single,
        french, //cannot use "double" as that is special world... so double doors are french
    }
    void Start()
    {
        Random.InitState(seed);
        // mesh = GetComponent<MeshFilter>().mesh; //do not remove
        set_maps();
       

        HashSet<int> seen_maps = new HashSet<int>();
        neighbors_to_roof = make_roof_dictionary();

        for (int i = 0; i < 3; i++) {       //run three times to generate 3 buildings. 
            int building_offset = i * 150;
            int map_index = Random.Range(0, num_maps);
            while (seen_maps.Contains(map_index)) map_index = Random.Range(0, num_maps - 1);    //get new map
            seen_maps.Add(map_index); //add new map to seen list.
            int [,] selected_map = maps[map_index];

          
            generate_building(selected_map, grid_size, building_offset);
            
        }

        // generate_building(maps[4], grid_size, 0);

        // GameObject window = generate_window(new Vector3(0,0,0), new Vector3(0,0,0), get_texture_blue_window());
        // window.transform.Rotate(new Vector3(0, 180, 0));
        // window.transform.Translate(new Vector3(1, 0, 0), Space.World);
        // window.transform.Translate(new Vector3(0, 0, 1), Space.World);
        // rotate_window_left(window);
        // rotate_window_left(window);

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
       
        // GameObject roof_1 = generate_cross_hip_roof(new Vector3(0, 0, 0), new Vector3(0,0,0));
        // GameObject roof_2 = generate_normal_roof();
        // roof_2.transform.Rotate(new Vector3(0, 90, 0));
        // roof_2.transform.Translate(new Vector3(-grid_size, 0, -grid_size / 2));
        // roof_go.transform.Translate(new Vector3(- grid_size  / 2, 0,  -grid_size / 2));
        // rotate_right(roof_1);
        // rotate_left(roof_1);
        // rotate_left(roof_1);

        // generate_building(maps[6], grid_size, 0);
        // generate_cross_hip_roof(new Vector3(), new Vector3());
        // generate_double_door(new Vector3(0,0,0), new Vector3(0,0,0), get_texture_checkerboard()).transform.Translate(new Vector3(0, 1, 0));

    }

    //generate roofs.
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

        // rend.material.color = Color.green;
        rend.material.mainTexture = get_texture_stone();
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

        // rend.material.color = Color.green;
        rend.material.mainTexture = get_texture_stone();
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

        // rend.material.color = Color.green;
        rend.material.mainTexture = get_texture_stone();
        // s.transform.Rotate(new Vector3(0, 90, 0));
        roof_go = s;
        return s;
    }

    //generate windows

    GameObject generate_window_wrapper(Texture2D texture, window_type win) {
        if (win == window_type.square) {
            return generate_window(new Vector3(0,0,0), new Vector3(0,0,0), texture);
        } else if (win == window_type.diamond) {
            return generate_window_round(new Vector3(0,0,0), new Vector3(0,0,0), texture);
        }
        return null;    //should never do so.
    }
    GameObject generate_window(Vector3 rotate, Vector3 translate, Texture2D texture) {
        Vector3[] verts = {
            new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(1, 1, 0), new Vector3(1, 0, 0),
        };

        int[] tris = {
            0, 1, 2, 0, 2, 3,
        };

        Vector2[] uvs = {
            new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0),
        };

        Mesh window = new Mesh();
        window.vertices = verts;
        window.triangles = tris;
        window.uv = uvs;
        window.RecalculateNormals();
    
         GameObject s = new GameObject("window");
        s.AddComponent<MeshFilter>();
        s.AddComponent<MeshRenderer>();
        
        // associate the mesh with this object
        s.GetComponent<MeshFilter>().mesh = window;

        // change the color of the object
        Renderer rend = s.GetComponent<Renderer>();

        rend.material.mainTexture = texture;
        return s;
    }

    GameObject generate_window_round(Vector3 rotate, Vector3 translate, Texture2D texture) {
        Vector3[] verts = {
            new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(1, 0, 0), new Vector3(0, -1, 0), new Vector3(-1, 0, 0)
        };

        int[] tris = {
            0, 1, 2, 
            0, 2, 3,
            0, 3, 4,
            0, 4, 1,
        };

        Vector2[] uvs = {
            new Vector2(0.5f, 0.5f), new Vector2(0, 1), new Vector2(1, 0), new Vector2(0, 0), new Vector2(0,0),
        };

        Mesh window = new Mesh();
        window.vertices = verts;
        window.triangles = tris;
        window.uv = uvs;
        window.RecalculateNormals();
    
         GameObject s = new GameObject("window");
        s.AddComponent<MeshFilter>();
        s.AddComponent<MeshRenderer>();
        
        // associate the mesh with this object
        s.GetComponent<MeshFilter>().mesh = window;

        // change the color of the object
        Renderer rend = s.GetComponent<Renderer>();

        rend.material.mainTexture = texture;
        return s;
    }

    GameObject generate_door_wrapper(Texture2D texture, door_type door) {
        if (door == door_type.single) {
            return generate_door(new Vector3(), new Vector3(), texture);
        } else if (door == door_type.french) {
            return generate_double_door(new Vector3(), new Vector3(), texture);
        }
        return null;
    }
    //generate doors
    GameObject generate_door(Vector3 rotate, Vector3 translate, Texture2D texture) {
         Vector3[] verts = {
            new Vector3(0, 0, 0), new Vector3(0, 2, 0), new Vector3(1, 2, 0), new Vector3(1, 0, 0),
        };

        int[] tris = {
            0, 1, 2, 0, 2, 3,
        };

        Vector2[] uvs = {
            new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0),
        };

        Mesh door = new Mesh();
        door.vertices = verts;
        door.triangles = tris;
        door.uv = uvs;
        door.RecalculateNormals();
    
         GameObject s = new GameObject("door");
        s.AddComponent<MeshFilter>();
        s.AddComponent<MeshRenderer>();
        
        // associate the mesh with this object
        s.GetComponent<MeshFilter>().mesh = door;

        // change the color of the object
        Renderer rend = s.GetComponent<Renderer>();

        rend.material.mainTexture = texture;
        return s;
    }

    GameObject generate_double_door(Vector3 rotate, Vector3 translate, Texture2D texture) {
        GameObject temp = generate_door(rotate, translate, texture);
        Mesh mesh = temp.GetComponent<MeshFilter>().mesh;
        Vector3[] verts = mesh.vertices;
        verts[2].x *= 2;
        verts[3].x *= 2;
        mesh.vertices = verts;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        return temp;
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

    //puts on left wall            
    void rotate_window_right(GameObject gm) {
        gm.transform.Rotate(new Vector3(0, 90, 0));
        gm.transform.Translate(new Vector3(-1, 0, 0));
    }

    //puts on right wall
    void rotate_window_left(GameObject gm) {
        gm.transform.Rotate(new Vector3(0, -90, 0));
        gm.transform.Translate(new Vector3(0, 0, -1));
    }

    
    //generate a single building.
    void generate_building(int [,] selected_map, int grid_size, int building_offset) {
        Texture2D wall_texture;
        if (true) {
            // wall_texture = get_texture_brick();
            wall_texture = get_texture_brick();
        } else {
            wall_texture = get_texture_checkerboard();
        }

        Texture2D roof_texture;
        if (true) {
            roof_texture = get_texture_stone(); //i didn't set the uvs in this so its just going to be a solid color.
        } else {
            roof_texture = get_texture_checkerboard();
        }

        Texture2D window_texture;
        if (true) {
            window_texture = get_texture_blue_window(); //i didn't set the uvs in this so its just going to be a solid color.
        } else {
            window_texture = get_texture_checkerboard();
        }

        Texture2D door_texture;
        if (true) {
            door_texture = get_texture_brown_door();
        } else {
            door_texture = get_texture_checkerboard();
        }

        window_type win_type;
        if (true) {
            win_type = window_type.diamond;
        } else {
            win_type = window_type.diamond;
        }

        door_type door_t;
        if (true) {
            door_t = door_type.french;
        } else {
            door_t = door_type.french;
        }
        //loop for each floor.
        for (int row = 0; row < selected_map.GetLength(0); row++) {
                for (int col = 0; col < selected_map.GetLength(1); col++) {
                    if (selected_map[row, col] > 0) {  //maybe roll to see if next floor at this spot? then loop until see all zeroes. 
                        //generate random heights. then make walls that high in the positions. 
                        //generate random heights with max height. then run algo max_height times and subtract one from each nonzero spot.
                        mesh_to_game_object(make_grid(grid_size, grid_size,  col * grid_size + building_offset, -row * grid_size, direction.floor), wall_texture);
                        if (left_wall_possible(selected_map, row, col)) {
                            mesh_to_game_object(make_grid(grid_size, grid_size, col * grid_size + building_offset, -row * grid_size, direction.left), wall_texture);
                            GameObject window = generate_window_wrapper(window_texture, win_type);
                            rotate_right(window);
                            window.transform.Translate(new Vector3(col * grid_size + building_offset - 0.01f, grid_size / 2, - (row) * grid_size - grid_size / 2) , Space.World);
                        } 
                        if (right_wall_possible(selected_map, row, col)) {
                            mesh_to_game_object(make_grid(grid_size, grid_size, (col + 1) * grid_size + building_offset, -row * grid_size, direction.right), wall_texture);
                            GameObject window = generate_window_wrapper(window_texture, win_type);
                            rotate_left(window);
                            window.transform.Translate(new Vector3((col) * grid_size + building_offset + 0.01f, grid_size / 2, - (row) * grid_size + grid_size / 2) , Space.World);
                        }
                        if (top_wall_possible(selected_map, row, col)) {
                            mesh_to_game_object(make_grid(grid_size, grid_size, col * grid_size + building_offset, - (row - 1) * grid_size, direction.top), wall_texture);
                            Vector3 offset = new Vector3(0, 0, 0.01f);
                            GameObject window = generate_window_wrapper(window_texture, win_type);
                            rotate_window_left(window);
                            rotate_window_left(window);
                            window.transform.Translate(new Vector3(col * grid_size + building_offset + grid_size / 2, grid_size / 2, - (row - 1) * grid_size - 1 + 0.01f) , Space.World);

                        } if (bottom_wall_possible(selected_map, row, col)) {
                            mesh_to_game_object(make_grid(grid_size, grid_size, col * grid_size + building_offset, - row * grid_size, direction.bottom), wall_texture);
                                GameObject window = generate_window_wrapper(window_texture, win_type);
                                window.transform.Translate(new Vector3(col * grid_size + building_offset + grid_size / 2, grid_size / 2, - (row) * grid_size - 0.01f) , Space.World);
                            if (Random.value > 0.5f) {
                                GameObject door = generate_door_wrapper(door_texture, door_t);
                                door.transform.localScale = new Vector3(2,2,2);
                                door.transform.Translate(new Vector3(col * grid_size + building_offset + grid_size / 2, 0, - (row) * grid_size - 0.01f) , Space.World);
                                window.transform.Translate(new Vector3(0, 1.25f, 0), Space.World);
                            }
                            
                            
                        }
                        int[] neighbors = get_neighbors(selected_map, row, col);
                        GameObject roof = Instantiate(neighbors_to_roof[neighbors]);    //make a copy of the selected roof
                        roof.GetComponent<Renderer>().material.mainTexture = roof_texture;    //this is a sloppy way to change the roof texture. it should be a parameter when making them.
                        roof.SetActive(true);
                        roof.transform.Translate(new Vector3(col * grid_size + building_offset, grid_size, -row * grid_size), Space.World);
                    }
                }
            }
    }

    //check if specific wall type is possible in "map" at (row, col)
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
    
    
    //set the floor plans for the map array
    void set_maps() {
        maps = new int[num_maps][,];
        int [,] map = new int[,] {
            {1, 1, 1}, 
            {1, 0, 1}, 
            {1, 1, 1}
        };

        int [,] map2 = new int[,] {
            {1, 1, 1, 1},
            {1, 0, 0, 1},
            {1, 1, 1, 1},
            {1, 0, 0, 1}
        };

        int [,] map3 = new int[,] {
            {1, 1, 1, 1, 1},
            {1, 0, 1, 0, 1},
            {1, 0, 1, 0, 1},
            {1, 0, 1, 0, 1},
            {1, 0, 1, 0, 1},
        };

        int [,] map4 = new int[,] {
            {1, 1, 1, 1, 1},
            {1, 0, 0, 0, 1},
            {1, 1, 1, 1, 1},
            {1, 0, 0, 0, 1},
            {1, 1, 1, 1, 1},
        };

        int [,] map5 = new int[,] {
            {1, 1, 1, 1, 1},
            {1, 0, 0, 0, 1},
            {1, 0, 0, 0, 1},
            {1, 0, 0, 0, 1},
            {1, 1, 0, 1, 1},
        };

        int [,] map6 = new int[,] {
            {1, 1, 1, 1, 1},
            {1, 0, 0, 0, 0},
            {1, 1, 1, 1, 1},
            {0, 0, 0, 0, 1},
            {1, 1, 1, 1, 1},
        };
        
        int [,] map7 = new int[,] {
            {1, 1, 1, 1, 1},
            {1, 0, 0, 0, 1},
            {1, 1, 1, 1, 1},
            {0, 0, 1, 0, 0},
            {0, 0, 1, 0, 0},
        };

        //S + D
        int [,] map8 = new int[,] {
            {1, 1, 1, 1, 0, 0, 0, 0, 0, 1, 1, 1, 0,},
            {1, 0, 0, 0, 0, 0, 1, 0, 0, 1, 0, 1, 1,},
            {1, 1, 1, 1, 0, 1, 1, 1, 0, 1, 0, 0, 1,},
            {0, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 1, 1,},
            {1, 1, 1, 1, 0, 0, 0, 0, 0, 1, 1, 1, 0,},
        };
        
        //<3
        int [,] map9 = new int[,] {
            {0, 1, 1, 1, 1, 0, 0, 0, 1, 1, 1, 1, 0,},
            {1, 1, 0, 0, 1, 1, 0, 1, 1, 0, 0, 1, 1,},
            {1, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 1,},
            {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1,},
            {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1,},
            {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1,},
            {1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1,},
            {0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0,},
            {0, 0, 1, 1, 0, 0, 0, 0, 0, 1, 1, 0, 0,},
            {0, 0, 0, 1, 1, 0, 0, 0, 1, 1, 0, 0, 0,},
            {0, 0, 0, 0, 1, 1, 0, 1, 1, 0, 0, 0, 0,},
            {0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0,},
            {0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0,},
        };

        maps[0] = map;
        maps[1] = map2;
        maps[2] = map3;
        maps[3] = map4;
        maps[4] = map5;
        maps[5] = map6;
        maps[6] = map7;
        maps[7] = map8;
        maps[8] = map9;
    }

    //take a position in the map, return the neighbors in the below order:
    //neighbors are ordered: [left, top, right, bottom]
    //if we are at the maps edge, then that neighbor will be counted as 0.
    int [] get_neighbors(int [,] map, int row, int col) {
        int[] nbs = new int[4];
        
        //left neighbor
        if (col == 0) {
            nbs[0] = 0;
        } else {
            nbs[0] = map[row, col - 1];
        }

        //top neighbor
        if (row == 0) {
            nbs[1] = 0;
        } else {
            nbs[1] = map[row - 1, col];
        }

        //right neighbor
        if (col == map.GetLength(1) - 1) {
            nbs[2] = 0;
        } else {
            nbs[2] = map[row, col + 1];
        }

        //bottom neighbor
        if (row == map.GetLength(0) - 1) {
            nbs[3] = 0;
        } else {
            nbs[3] = map[row + 1, col];
        }

        return nbs;
    }

    //create a dictionary that takes in a list of neighbors and outputs the roof object that should go at that position.
    //dictionary goes: [left, up, right, down] for the neighbor ordering
    Dictionary<int[], GameObject> make_roof_dictionary() {
        Dictionary<int[], GameObject> result = new Dictionary<int[], GameObject>(int_comp);
        int [] neighbors;
        GameObject roof;
        
        /* 4 NEIGHBORS */ /* Author's Note: This could also be remodeled. */
        //   1
        //1  1  1
        //   1
        neighbors = new int[] {1,1,1,1};
        roof = generate_normal_roof(new Vector3(0,0,0), new Vector3(0,0,0));
        roof.SetActive(false);
        result.Add(neighbors, roof);

        /* 3 NEIGHBORS */
        //   1
        //0  1  1
        //   1
        neighbors = new int[] {0,1,1,1};
        roof = generate_cross_gable_roof(new Vector3(0,0,0), new Vector3(0,0,0));
        rotate_left(roof);
        roof.SetActive(false);
        result.Add(neighbors, roof);

        //   1
        //1  1  1
        //   0
        neighbors = new int[] {1,1,1,0};
        roof = generate_cross_gable_roof(new Vector3(0,0,0), new Vector3(0,0,0));
        rotate_left(roof);
        rotate_left(roof);
        roof.SetActive(false);
        result.Add(neighbors, roof);

        //   1
        //1  1  0
        //   1
        neighbors = new int[] {1,1,0,1};
        roof = generate_cross_gable_roof(new Vector3(0,0,0), new Vector3(0,0,0));
        rotate_right(roof);
        roof.SetActive(false);
        result.Add(neighbors, roof);

        //   0
        //1  1  1
        //   1
        neighbors = new int[] {1,0,1,1};
        roof = generate_cross_gable_roof(new Vector3(0,0,0), new Vector3(0,0,0));
        roof.SetActive(false);
        result.Add(neighbors, roof);

        /* 2 NEIGHBORS */
        
        //   0
        //0  1  1
        //   1
        neighbors = new int[] {0,0,1,1};
        roof = generate_cross_hip_roof(new Vector3(0,0,0), new Vector3(0,0,0));
        rotate_left(roof);
        roof.SetActive(false);
        result.Add(neighbors, roof);

        //   1
        //0  1  0
        //   1
        neighbors = new int[] {0,1,0,1};
        roof = generate_normal_roof(new Vector3(0,0,0), new Vector3(0,0,0));
        roof.SetActive(false);
        result.Add(neighbors, roof);

        //   1
        //0  1  1
        //   0
        neighbors = new int[] {0,1,1,0};
        roof = generate_cross_hip_roof(new Vector3(0,0,0), new Vector3(0,0,0));
        rotate_left(roof);
        rotate_left(roof);
        roof.SetActive(false);
        result.Add(neighbors, roof);

        //   0
        //1  1  0
        //   1
        neighbors = new int[] {1,0,0,1};
        roof = generate_cross_hip_roof(new Vector3(0,0,0), new Vector3(0,0,0));
        roof.SetActive(false);
        result.Add(neighbors, roof);

        //   0
        //1  1  1
        //   0
        neighbors = new int[] {1,0,1,0};
        roof = generate_normal_roof(new Vector3(0,0,0), new Vector3(0,0,0));
        rotate_left(roof);
        roof.SetActive(false);
        result.Add(neighbors, roof);

        //   1
        //1  1  0
        //   0
        neighbors = new int[] {1,1,0,0};
        roof = generate_cross_hip_roof(new Vector3(0,0,0), new Vector3(0,0,0));
        rotate_right(roof);
        roof.SetActive(false);
        result.Add(neighbors, roof);

        /* 1 NEIGHBOR */

        //   0
        //1  1  0
        //   0
        neighbors = new int[] {1,0,0,0};
        roof = generate_normal_roof(new Vector3(0,0,0), new Vector3(0,0,0));
        rotate_right(roof);
        roof.SetActive(false);
        result.Add(neighbors, roof);

        //   0
        //0  1  1
        //   0
        neighbors = new int[] {0,0,1,0};
        roof = generate_normal_roof(new Vector3(0,0,0), new Vector3(0,0,0));
        rotate_right(roof);
        roof.SetActive(false);
        result.Add(neighbors, roof);

        //   1
        //0  1  0
        //   0
        neighbors = new int[] {0,1,0,0};
        roof = generate_normal_roof(new Vector3(0,0,0), new Vector3(0,0,0));
        roof.SetActive(false);
        result.Add(neighbors, roof);

        //   0
        //0  1  0
        //   1
        neighbors = new int[] {0,0,0,1};
        roof = generate_normal_roof(new Vector3(0,0,0), new Vector3(0,0,0));
        roof.SetActive(false);
        result.Add(neighbors, roof);

        /* 0 NEIGHBORS */   /* Author's Note: This could be remodeled. */
        //   0
        //0  1  0
        //   0
        neighbors = new int[] {0,0,0,0};
        roof = generate_normal_roof(new Vector3(0,0,0), new Vector3(0,0,0));
        roof.SetActive(false);
        result.Add(neighbors, roof);

        return result;
    }
    

    // Update is called once per frame
    void Update()
    {
    }

    //all are defined by bottom left corner and move in positive directions.
    //direction.floor = move in +x, +z
    //direction.horizontal = move in +x, +y
    //direciton.vertical = move in +y, +z
    //offsets move it in the xz plane
    private Mesh make_grid (int x_size, int y_size, float x_offset, float y_offset, direction d) {
		Mesh temp_mesh = new Mesh();
		

        Vector3[] verts = new Vector3[(x_size + 1) * (y_size + 1)];
        Vector2[] uvs = new Vector2[verts.Length];

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
                uvs[vert_index] = new Vector2((float) x / x_size, (float) y / y_size);
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
        //make sure the meshes are viewable from viewing angle.
        if (d == direction.top || d == direction.right || d == direction.floor) {
            System.Array.Reverse(tris);
        }
		temp_mesh.triangles = tris;
        temp_mesh.uv = uvs;
        return temp_mesh;
	}

    //texture is determined here.
    GameObject mesh_to_game_object(Mesh mesh, Texture texture) {
        
        mesh.RecalculateNormals();
        GameObject s = new GameObject("wall");
        s.AddComponent<MeshFilter>();
        s.AddComponent<MeshRenderer>();
        
        // associate the mesh with this object
        s.GetComponent<MeshFilter>().mesh = mesh;

        // change the texture of the object
        Renderer rend = s.GetComponent<Renderer>();
        rend.material.mainTexture = texture;
       
        return s;
    }

    //procedurally generated textures.
    Texture2D get_texture() {
        Texture2D image = new Texture2D(128, 128);
        for (int y = 0; y < image.height; y++) {
            for (int x = 0; x < image.width; x++) {
                Color color = Color.Lerp(Color.black, Color.white, (float)x / image.width);
                image.SetPixel(x, y ,color);
            }
        }
        image.Apply();
        return image;
    }

    Texture2D get_texture_checkerboard() {
        Texture2D image = new Texture2D(128, 128);
        int cell_size = 10;
        for (int y = 0; y < image.height; y++) {
            for (int x = 0; x < image.width; x++) {
                Color color = Color.Lerp(Color.black, Color.white, (float)x / image.width);
                if ((x / cell_size + y / cell_size) % 2 == 0) color = Color.black;
                else color = Color.white;
                image.SetPixel(x, y ,color);
            }
        }
        image.Apply();
        return image;
    }

    Texture2D get_texture_brick() {
        Texture2D image = new Texture2D(128, 128);
        for (int y = 0; y < image.height; y++) {
            for (int x = 0; x < image.width; x++) {
                Color color = Color.Lerp(Color.black, Color.white, (float)x / image.width);
                Color light = new Color(221.0f / 255, 125.0f / 255, 125.0f / 255);
                Color dark = new Color(132 / 255.0f, 32 / 255.0f, 32 / 255.0f);
                // color = Color.Lerp(light, dark, Mathf.PerlinNoise(2 * x, 2 * y) / 2);
                // color = Color.Lerp(light, dark, (float) x / image.height);
                color = Color.Lerp(light, dark, Random.value);
                image.SetPixel(x, y ,color);
            }
        }
        image.Apply();
        return image;
    }

    Texture2D get_texture_stone() {
        Texture2D image = new Texture2D(128, 128);
        for (int y = 0; y < image.height; y++) {
            for (int x = 0; x < image.width; x++) {
                Color light = new Color(164,172,180) / 255.0f;
                Color dark = new Color(134,152,166) / 255.0f;
                float control_knob = Random.value;
                control_knob = Mathf.Sqrt(control_knob);        //make probability dist. shift to right side
                Color color = Color.Lerp(light, dark, control_knob);
                image.SetPixel(x, y ,color);
            }
        }
        image.Apply();
        return image;
    }

    Texture2D get_texture_blue_window() {
        Texture2D image = new Texture2D(128, 128);
        for (int y = 0; y < image.height; y++) {
            for (int x = 0; x < image.width; x++) {
                Color dark = new Color(66, 89, 195) / 255.0f;
                Color light = new Color(158, 194, 255) / 255.0f;
                float control_knob = (float)(x * y) / (image.height * image.width);
                Color color = Color.Lerp(dark, light, control_knob);
                image.SetPixel(x, y ,color);
            }
        }
        image.Apply();
        return image;
    }

    Texture2D get_texture_brown_door() {
       int cell_size = 10;
       Texture2D image = new Texture2D(128, 128);
        for (int y = 0; y < image.height; y++) {
            for (int x = 0; x < image.width; x++) {
                Color color;
                Color dark = new Color(110, 38, 14) / 255.0f;
                Color light = new Color(233, 116, 81) / 255.0f;
                if ((x / cell_size) % 2 == 0) color = dark;
                else color = light;
                // float control_knob = (float)(x * y) / (image.height * image.width);
                // Color color = Color.Lerp(dark, light, control_knob);
                image.SetPixel(x, y ,color);
            }
        }
        image.Apply();
        return image;
    }

    //generate a random_int from 0 to n - 1
    private int random_int(int n) {
        return (int) (Random.value * n);
    }

    //generate a random int from min to max - 1
    private int random_int(int min, int max) {
        return random_int(max - min) + min;
    }

    //cute little test function :), has no roof :(
    void test_house() {
        Texture texture = get_texture_checkerboard();
        mesh_to_game_object(make_grid(grid_size, grid_size, 0, 0, direction.floor), texture);
        mesh_to_game_object(make_grid(grid_size, grid_size, 0, 0, direction.bottom), texture);
        mesh_to_game_object(make_grid(grid_size, grid_size, 0, 0, direction.left), texture);
        mesh_to_game_object(make_grid(grid_size, grid_size, 0, grid_size, direction.top), texture);
        mesh_to_game_object(make_grid(grid_size, grid_size, grid_size, 0, direction.right), texture);        
    }  
    //Helpful in ensuring the grid is properly sized. 
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
}


class IntArrayEqualityComparer : IEqualityComparer<int[]>
{
    public bool Equals(int[] x, int[] y)
    {
        if (x == null || y == null || x.Length != y.Length) return false; 
        
        for (int i = 0; i < x.Length; i++)
        {
            if (x[i] != y[i])
            {
                return false;
            }
        }
        return true;
    }

    //hash function, made fairly arbitrarily using some primes?? 
    public int GetHashCode(int[] obj)
    {
        
        int result = 233;
        for (int i = 0; i < obj.Length; i++)
        {
            result = result * 47 + obj[i];
        }
        return result;
    }
}